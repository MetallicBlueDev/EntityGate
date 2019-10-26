using MetallicBlueDev.EntityGate.InterfacedObject;

namespace MetallicBlueDev.Sample.Data
{
    public partial class SampleEntity1 : IEntityObjectIdentifier
    {
        /// <summary>
        /// Mandatory implementation of the <see cref="IEntityObjectIdentifier"/> interface.
        /// </summary>
        public object Identifier => Id;
    }
}
