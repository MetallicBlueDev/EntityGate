# EntityGate
EntityGate is an open source .NET library to easily use Entity Framework in your applications.

This microframework allows you to use objects managed by Entity Framework without worrying about the context.

# Is EntityGate for me?
Although EntityGate can be used by all, it exists specifically to offer additional scenarios to the Entity Framework.

- You want to use the Entity Framework and all its tools.
- You want to serialize an Entity (with its modifications).
- You prefer to work from an object point of view without worrying about the context.
- You want to manage an Entity without knowing its context.
- You want to manage a context without knowing the entity to handle.
- You are not familiar with the concept of context.
- Your application is not context-oriented.
- You want a manipulation of the entities and their contexts more transparent to you.

__If one or more scenarios answer your request, this library might interest you.__

# Example

```
// Creation of a new entity.
SampleEntity1 sample1 = new SampleEntity1();
sample1.Value = 3;

// EntityGate support and database registration.
EntityGate<> gate = new EntityGate<SampleEntity1>(sample1);

if (gate.Save()) {
  // New data saved in database.
}

// Creation of a new entity.
gate.NewEntity();
gate.Value = 4;
gate.Save();

// Loading the primary key "10" and obtaining the entity.
gate.Load(pKeyValue=10);
sample1 = gate.Entity;

// Support for another entity.
EntityGate<> gate = new EntityGate<OtherEntity>(sample1);

// Loading the primary key "33"...
if (gate.Load(33)) {
  // ...and obtaining the entity.
  OtherEntity sample2 = gate.Entity;
}
```

More example to come.