using React;

namespace Envisia.React.Extensions;

public class EnvisiaReactEnvironment : ReactEnvironment
{
    private readonly IReactIdGenerator _reactIdGenerator2;

    public EnvisiaReactEnvironment(
        IJavaScriptEngineFactory engineFactory,
        IReactSiteConfiguration config,
        ICache cache,
        IFileSystem fileSystem,
        IFileCacheHash fileCacheHash,
        IReactIdGenerator reactIdGenerator) : base(engineFactory, config, cache, fileSystem, fileCacheHash,
        reactIdGenerator)
    {
        _reactIdGenerator2 = reactIdGenerator;
    }

    public override IReactComponent CreateComponent<T>(
        string componentName, T props,
        string containerId = null,
        bool clientOnly = false,
        bool serverOnly = false,
        bool skipLazyInit = false)
    {
        if (!clientOnly)
        {
            EnsureUserScriptsLoaded();
        }

        var component = new EnvisiaReactComponent(this, _config, _reactIdGenerator2, componentName, containerId)
        {
            ClientOnly = clientOnly,
            Props = props,
            ServerOnly = serverOnly
        };

        if (!skipLazyInit)
        {
            _components.Add(component);
        }

        return component;
    }
}