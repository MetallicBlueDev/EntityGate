using System.Configuration;
using System.Xml;

namespace MetallicBlueDev.EntityGate.Configuration
{
    /// <summary>
    /// EntityGate configuration section.
    /// </summary>
    public class EntityGateSectionHandler : IConfigurationSectionHandler
    {
        /// <summary>
        /// Creating the EntityGate configuration.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            if (!EntityGateConfigLoader.Initialized())
            {
                EntityGateConfigLoader.Initialize(section);
            }

            return EntityGateConfigLoader.GetConfigs();
        }
    }
}

