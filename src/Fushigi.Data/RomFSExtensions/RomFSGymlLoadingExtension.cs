﻿using System.Collections.Immutable;
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
    Task OnGymlTypeMismatch(LoadedGymlTypeMismatchErrorInfo info);
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
            //no need to allocate anything until we actually have an inheritance chain
            ImmutableStack<(string, RomFS.RetrievedFileLocation)>.Empty, 
            errorHandler,
            pack);
    }

    private class GymlManager(RomFS romFS)
    {
        private class ChainLink<T>(T value)
        {
            public ChainLink<T>? Successor = null;
            public T Value = value;
        }
        
        private readonly Dictionary<string, (object instance, RomFS.RetrievedFileLocation fileLocation)> _loadedGymlFiles = [];
        
        public async Task<(bool success, T?)> LoadGyml<T>(GymlRef<T> gymlRef,
            //only works with recursion
            ImmutableStack<(string, RomFS.RetrievedFileLocation)> inheritanceChain, 
            IGymlFileLoadingErrorHandler errorHandler,
            PackInfo? pack = null)
            where T : GymlFile<T>, IGymlType, new()
        {
            if (_loadedGymlFiles.TryGetValue(gymlRef.ValidatedRefPath, out var alreadyLoadedGyml))
            {
                if (alreadyLoadedGyml.instance is not T alreadyLoadedGymlT)
                {
                    await errorHandler.OnGymlTypeMismatch(
                        new LoadedGymlTypeMismatchErrorInfo(gymlRef.ValidatedRefPath, 
                            typeof(T), alreadyLoadedGyml.instance.GetType(), 
                            alreadyLoadedGyml.fileLocation)
                    );
                    return (false, null);
                }
                return (true, (T?)alreadyLoadedGymlT);
            }

            string[]? filePath = FileRefConversion.GetRomFSFilePath(gymlRef);
            
            if (await romFS.LoadFile(filePath, FormatDescriptors.BymlUncompressed, 
                    errorHandler, 
                    pack
                ) is not (true, {} loadedByml, var fileLocation))
            {
                return (false, null);
            }
            
            if (await GymlFile<T>.DeserializeFrom(loadedByml, errorHandler, fileLocation)
                is not (true, { } loadedGyml))
            {
                return (false, null);
            }

            if (loadedGyml.ParentGymlRef is {} parentGymlRef)
            {
                inheritanceChain = inheritanceChain.Push((gymlRef.ValidatedRefPath, fileLocation));
                
                if (inheritanceChain.Contains((parentGymlRef.ValidatedRefPath, fileLocation)))
                {
                    //we were about to load a gyml that's already in our inheritance chain
                    var inheritanceChainArray = inheritanceChain.ToArray();
                    Array.Reverse(inheritanceChainArray); //stack is iterated in reverse so we need to counter that
                    int cycleBeginIndex = Array.IndexOf(inheritanceChainArray, (parentGymlRef.ValidatedRefPath, fileLocation));
                    await errorHandler.OnCyclicInheritance(new CyclicInheritanceErrorInfo(inheritanceChainArray, cycleBeginIndex));
                    return (false, null);
                }
                
                if (await LoadGyml<T>(parentGymlRef,
                        inheritanceChain,
                        errorHandler,
                        pack) is not (true, {} loadedParentGyml))
                {
                    return (false, null);
                }
                
                loadedGyml.SetParent(loadedParentGyml);
            }

            //only add to loaded after we loaded the entire inheritance chain and made sure there are no cycles
            _loadedGymlFiles[gymlRef.ValidatedRefPath] = (loadedGyml, fileLocation);
        
            return (true, loadedGyml);
        }
    }
}