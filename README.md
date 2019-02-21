# Intro

This repository is basically a research repository for [EntityFramework6](https://www.nuget.org/packages/EntityFramework)
together with [System.Data.SQLite.EF6](https://www.nuget.org/packages/System.Data.SQLite.EF6/) SQLite.

# SQLite-specific knowledge

## Many-to-many collection navigation properties aren't working


# Non-SQLite specific knowledge

## Does EF6 allow me to use custom collections?

When using navigation properties: Yes! You can even use `ICollection` as type and initialize with any collection
implementing `ICollection`. Example:

```
public ICollection<Type> Foo { get; set; } = new ObservableCollection<Type>();
public ICollection<Type> Bar { get; set; } = new List<Type>();
```

You could even implement a derived class which temporarily disables `INotifyPropertyChanged` while loading data using
EF6.

You should avoid using `DbSet<>.Local`, as this will 