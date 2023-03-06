namespace Camunda.Connector.SDK.Runtime.Util;

public interface ConnectorFactory<T, C>
{
    /**
  * List all available configurations loaded by the runtime
  *
  * @return List of available configurations
  */
    List<C> GetConfigurations();

    /**
     * Create a Connector instance by type
     *
     * @param type Connector type
     * @return Connector instance
     */
    T GetInstance(string type);

    /**
     * Dynamically register a new Connector configuration. If a connector with the same type already
     * exists, it will be overridden by the new configuration.
     *
     * @param configuration Configuration to register
     */
    void RegisterConfiguration(C configuration);

    /** Reload all connectors from classpath and reset all manually registered connectors */
    void ResetConfigurations();
}