using System.Collections.Immutable;
using Fushigi.Data.Files;
using Fushigi.Data.Files.GymlTypes;

namespace Fushigi.Data.RomFSExtensions;

public record LoadedGymlTypeMismatchErrorInfo(string GymlPath, Type Expected, Type AlreadyLoaded);
public record CyclicInheritanceErrorInfo(string[] InheritanceCycle);

public interface IGymlFileLoadingErrorHandler : IFileResolutionAndLoadingErrorHandler, IFileRefPathErrorHandler
{
    Task OnGymlTypeMismatch(LoadedGymlTypeMismatchErrorInfo info);
    Task OnCyclicInheritance(CyclicInheritanceErrorInfo info);
}

public static class RomFSGymlLoadingExtension
{
    private static readonly Dictionary<RomFS, GymlManager> s_gymlManagerLookup = [];
    
    public static async Task<(bool success, T?)> LoadGyml<T>(this RomFS romFS, string gymlPath,
        IGymlFileLoadingErrorHandler errorHandler,
        PackInfo? pack = null)
        where T : GymlFile<T>, new()
    {
        if (!s_gymlManagerLookup.TryGetValue(romFS, out var gymlManager))
        {
            gymlManager = new GymlManager(romFS);
            s_gymlManagerLookup[romFS] = gymlManager;
        }

        return await gymlManager.LoadGyml<T>(gymlPath,
            ImmutableList<string>.Empty, //no need to allocate anything until we actually have an inheritance chain
            errorHandler,
            pack);
    }

    private class GymlManager(RomFS romFS)
    {
        private readonly Dictionary<string, object> _loadedGymlFiles = [];
        private static readonly (string inProdution, string shipped) s_gymlSuffix = (".gyml", ".bgyml");
        
        public async Task<(bool success, T?)> LoadGyml<T>(string gymlPath,
            ImmutableList<string> inheritanceChain, //only works with recursion, stack like
            IGymlFileLoadingErrorHandler errorHandler,
            PackInfo? pack = null)
            where T : GymlFile<T>, new()
        {
            if (_loadedGymlFiles.TryGetValue(gymlPath, out object? alreadyLoadedGyml))
            {
                if (alreadyLoadedGyml is not T alreadyLoadedGymlT)
                {
                    await errorHandler.OnGymlTypeMismatch(
                        new LoadedGymlTypeMismatchErrorInfo(gymlPath, typeof(T), alreadyLoadedGyml.GetType())
                    );
                    return (false, null);
                }
                return (true, (T?)alreadyLoadedGymlT);
            }

            if (await FileRefPathConversion.GetRomFSFilePath(gymlPath, s_gymlSuffix,
                    errorHandler) is not (true, { } filePath))
            {
                return (false, null);
            }
            
            if (await romFS.LoadFile(filePath, FormatDescriptors.GetGymlFormat<T>(), 
                    errorHandler, 
                    pack
                ) is not (true, {} loadedGyml))
            {
                return (false, null);
            }

            if (loadedGyml.ParentGymlRefString != null)
            {
                inheritanceChain = inheritanceChain.Add(gymlPath);
                
                if (inheritanceChain.Contains(loadedGyml.ParentGymlRefString))
                {
                    //we were about to load a gyml that's already in our inheritance chain
                    await errorHandler.OnCyclicInheritance(new CyclicInheritanceErrorInfo(inheritanceChain.ToArray()));
                    return (false, null);
                }
                
                if (await LoadGyml<T>(loadedGyml.ParentGymlRefString,
                        inheritanceChain,
                        errorHandler,
                        pack) is not (true, {} loadedParentGyml))
                {
                    return (false, null);
                }
                
                loadedGyml.SetParent(loadedParentGyml);
            }

            //only add to loaded after we loaded the entire inheritance chain and made sure there are no cycles
            _loadedGymlFiles[gymlPath] = loadedGyml;
        
            return (true, loadedGyml);
        }
    }
}