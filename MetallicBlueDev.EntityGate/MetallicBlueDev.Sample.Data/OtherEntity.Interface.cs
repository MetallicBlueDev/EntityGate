using MetallicBlueDev.EntityGate.InterfacedObject;

namespace MetallicBlueDev.Sample.Data
{
    public partial class OtherEntity : IEntityObjectIdentifier
    {
        /// <summary>
        /// Mandatory implementation of the <see cref="IEntityObjectIdentifier"/> interface.
        /// </summary>
        public object Identifier => Id;
    }
}
