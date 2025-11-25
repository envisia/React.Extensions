using React;

namespace Envisia.React.Extensions;

#nullable enable

public class React18Environment(
    IJavaScriptEngineFactory engineFactory,
    IReactSiteConfiguration config,
    ICache cache,
    IFileSystem fileSystem,
    IFileCacheHash fileCacheHash,
    IReactIdGenerator reactIdGenerator)
    : ReactEnvironment(engineFactory,
        config,
        cache,
        fileSystem,
        fileCacheHash,
        reactIdGenerator)
{
    private readonly IReactIdGenerator _reactIdGenerator = reactIdGenerator;

    /// <summary>
    /// Creates an instance of the specified React JavaScript component.
    /// </summary>
    /// <typeparam name="T">Type of the props</typeparam>
    /// <param name="componentName">Name of the component</param>
    /// <param name="props">Props to use</param>
    /// <param name="containerId">ID to use for the container HTML tag. Defaults to an auto-generated ID</param>
    /// <param name="clientOnly">True if server-side rendering will be bypassed. Defaults to false.</param>
    /// <param name="serverOnly">True if this component only should be rendered server-side. Defaults to false.</param>
    /// <param name="skipLazyInit">Skip adding to components list, which is used during GetInitJavascript</param>
    /// <returns>The component</returns>
    public override IReactComponent CreateComponent<T>(
        string componentName,
        T props,
        string? containerId = null,
        bool clientOnly = false,
        bool serverOnly = false,
        bool skipLazyInit = false)
    {
        if (!clientOnly)
        {
            EnsureUserScriptsLoaded();
        }

        var component = new React18Component(this, _config, _reactIdGenerator, componentName, containerId)
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