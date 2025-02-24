using System.Collections.Immutable;
using System.Diagnostics;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using Fushigi.Data.BymlSerialization;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;

namespace Fushigi.Data.RomFSExtensions;

public record LoadedGymlTypeMismatchErrorInfo(string GymlPath, Type Expected, Type AlreadyLoaded, 
    RomFS.RetrievedFileLocation AlreadyLoadedFileLocation);
/// <summary>
/// 
/// </summary>
/// <param name="InheritanceChain">The inheritance chain up to where the cycle was detected
/// (Cycle is caused by the last item's parent ref) </param>
/// <param name="CycleBeginIdx">The index of the item that is referenced by the last item, causing the cycle</param>
public record CyclicInheritanceErrorInfo((string gymlPath, RomFS.RetrievedFileLocation)[] InheritanceChain, 
    int CycleBeginIdx);

public interface IGymlFileLoadingErrorHandler : 
    IFileResolutionAndLoadingErrorHandler, 
    IBymlDeserializeErrorHandler
{
    Task OnCyclicInheritance(CyclicInheritanceErrorInfo info);
}

public static class RomFSGymlLoadingExtension
{
    private static readonly Dictionary<RomFS, GymlManager> s_gymlManagerLookup = [];
    
    public static async Task<(bool success, T?)> LoadGyml<T>(this RomFS romFS, GymlRef<T> gymlRef,
        IGymlFileLoadingErrorHandler errorHandler,
        PackInfo? pack = null)
        where T : GymlFile<T>, IGymlType, new()
    {
        if (!s_gymlManagerLookup.TryGetValue(romFS, out var gymlManager))
        {
            gymlManager = new GymlManager(romFS);
            s_gymlManagerLookup[romFS] = gymlManager;
        }

        return await gymlManager.LoadGyml(gymlRef, 
            errorHandler,
            pack);
    }

    private class GymlManager(RomFS romFS)
    {
        private readonly Dictionary<string, (object instance, RomFS.RetrievedFileLocation fileLocation)> _loadedGymlFiles = [];
        
        public async Task<(bool success, T?)> LoadGyml<T>(GymlRef<T> gymlRef,
            IGymlFileLoadingErrorHandler errorHandler,
            PackInfo? pack = null)
            where T : GymlFile<T>, IGymlType, new()
        {
            if (_loadedGymlFiles.TryGetValue(gymlRef.ValidatedRefPath, out var alreadyLoadedGyml))
                return (true, (T?)alreadyLoadedGyml.instance);

            if (await LoadGymlHierarchy(gymlRef, ImmutableStack<InheritanceChainItem>.Empty, 
                errorHandler, pack) is not (true, {} gymlHierarchy))
                return (false, null);

            var foundPropertyPaths = new HashSet<PropertyPath>();
            foreach (var (_, _, loadedByml) in gymlHierarchy)
                CollectProperties(loadedByml, foundPropertyPaths, ImmutableStack<string>.Empty);

            T? firstInChain = null;
            T? previousInChain = null;
            foreach ((string gymlPath, var fileLocation, var loadedByml) in gymlHierarchy)
            {
                if (await GymlFile<T>.DeserializeFrom(loadedByml, errorHandler, fileLocation, foundPropertyPaths)
                    is not (true, { } loadedGyml))
                {
                    return (false, null);
                }
                
                _loadedGymlFiles[gymlPath] = (loadedGyml, fileLocation);

                if (previousInChain != null &&
                    previousInChain.ParentGymlRef?.ValidatedRefPath != gymlPath)
                {
                    Debug.Fail("Something went wrong");
                }
                previousInChain?.SetParent(loadedGyml);

                firstInChain ??= loadedGyml;
                previousInChain = loadedGyml;
            }
        
            return (true, firstInChain);
        }
        
        private async Task<(bool success, InheritanceChainItem[]? bgymls)> LoadGymlHierarchy<T>(GymlRef<T> gymlRef,
            //only works with recursion
            ImmutableStack<InheritanceChainItem> inheritanceChain,
            IGymlFileLoadingErrorHandler errorHandler,
            PackInfo? pack = null)
            where T : GymlFile<T>, IGymlType, new()
        {
            string[]? filePath = FileRefConversion.GetRomFSFilePath(gymlRef);
            
            if (await romFS.LoadFile(filePath, FormatDescriptors.BymlUncompressed, 
                    errorHandler, 
                    pack
                ) is not (true, {} loadedByml, var fileLocation))
            {
                return (false, null);
            }
            
            inheritanceChain = inheritanceChain.Push(
                new InheritanceChainItem(gymlRef.ValidatedRefPath, fileLocation, loadedByml)
            );

            // this code tries to retrieve the $parent property without throwing or reporting errors.
            // errors in type or formatting will be reported later by GymlFile<T>.Serialization.
            if (loadedByml.Value is BymlMap rootMap && 
                rootMap.TryGetValue("$parent", out var parentValueNode) &&
                parentValueNode.Value is string parentGymlPath &&
                FileRefConversion.IsValid<GymlRef<T>>(parentGymlPath))
            {
                var parentGymlRef = FileRefConversion.Parse<GymlRef<T>>(parentGymlPath);
                
                if (TryFindGymlInChain(inheritanceChain, parentGymlPath, out var foundItem))
                {
                    //we were about to load a gyml that's already in our inheritance chain
                    var inheritanceChainArray = inheritanceChain.ToArray();
                    Array.Reverse(inheritanceChainArray); //stack is iterated in reverse so we need to counter that
                    
                    int cycleBeginIndex = Array.IndexOf(inheritanceChainArray, foundItem);
                    
                    await errorHandler.OnCyclicInheritance(new CyclicInheritanceErrorInfo(
                        Array.ConvertAll(inheritanceChainArray, item => (item.GymlPath, item.FileLocation)), 
                        cycleBeginIndex
                    ));
                    return (false, null);
                }

                var recursionReturn = await LoadGymlHierarchy(parentGymlRef,
                    inheritanceChain,
                    errorHandler,
                    pack);
                
                return recursionReturn is (true, {} gymls) ? (true, gymls) : (false, null);
            }
        
            InheritanceChainItem[] returnArray;
            {
                returnArray = new InheritanceChainItem[inheritanceChain.Count()];
                var idx = 0;
                foreach (var item in inheritanceChain) 
                    returnArray[idx++] = item;

                Array.Reverse(returnArray); //stack is iterated in reverse so we need to counter that
            }
            
            return (true, returnArray);
        }
        
        private static void CollectProperties(Byml node, HashSet<PropertyPath> foundPropertyPaths, 
            ImmutableStack<string> propertyPathStack)
        {
            if (node.Value is not BymlMap bymlMap)
                return;
            
            foreach ((string key, var value) in bymlMap)
            {
                var keyPathStack = propertyPathStack.Push(key);
                foundPropertyPaths.Add(new PropertyPath(keyPathStack));
                CollectProperties(value, foundPropertyPaths, keyPathStack);
            }
        }

        private record struct InheritanceChainItem(string GymlPath, RomFS.RetrievedFileLocation FileLocation, 
            Byml LoadedByml);

        private static bool TryFindGymlInChain(ImmutableStack<InheritanceChainItem> inheritanceChain, string gymlPath, 
            out InheritanceChainItem foundItem)
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var item in inheritanceChain)
            {
                if (item.GymlPath != gymlPath) continue;
                foundItem = item;
                return true;
            }
            foundItem = default;
            return false;
        }
    }
}