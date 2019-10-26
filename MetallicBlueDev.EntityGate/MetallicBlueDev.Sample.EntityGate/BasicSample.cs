using MetallicBlueDev.EntityGate;
using MetallicBlueDev.Sample.Data;

namespace MetallicBlueDev.Sample.EntityGate
{
    class BasicSample
    {
        public void Test()
        {
            // Creation of a new entity.
            var sample1 = new SampleEntity1();
            sample1.Value = 3;

            //// EntityGate support and database registration.
            var gate = new EntityGateObject<SampleEntity1>(sample1);

            if (gate.Save())
            {
                // New data saved in database.
            }

            // Creation of a new entity.
            gate.NewEntity();
            gate.Entity.Value = 4;
            gate.Save();

            // Loading the primary key "10" and obtaining the entity.
            gate.Load(identifier: 10);
            sample1 = gate.Entity;

            // Support for another entity.
            var gate2 = new EntityGateObject<OtherEntity>();

            // Loading the primary key "33"...
            if (gate2.Load(33))
            {
                // ...and obtaining the entity.
                OtherEntity sample2 = gate2.Entity;
            }
        }
    }
}
