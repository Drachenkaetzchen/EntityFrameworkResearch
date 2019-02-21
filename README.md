# Intro

This repository is basically a research repository for [EntityFramework6](https://www.nuget.org/packages/EntityFramework)
together with [System.Data.SQLite.EF6](https://www.nuget.org/packages/System.Data.SQLite.EF6/) SQLite.

# SQLite-specific knowledge

## Many-to-many collection navigation properties aren't working

The best case scenario would be to load all `Plugins` with all joins like `Modes`, `Types`, `Presets`,
`Presets.Types`, `Presets.Modes` etc in one go. However, as illustrated in the `NestedManyToManyIncludeSample`
and also [asked on StackOverFlow](https://stackoverflow.com/questions/54802806/including-multiple-many-to-many-collection-navigation-properties),
there seems to be a bug or oversight or edge-case or missing implementation to make this possible.
 
## Benchmarks

To work around the *Many-to-many collection navigation properties* problem, I have experimented with different
loading mechanisms, including simply loading all (or most) data into `DbSets`. This is actually the use case I
have (see [Motivation](#motivation) for a very verbose requirements explanation).

The main benchmark right now is to load the 4 main `DbSets` using different `DbContextConfiguration` flags,
which is a total of 76k entities. The benchmark also ensures that retrieved data is correctly mapped by checking
the collection counts (which probably takes longer than the loading itself).


### Results

These results were created on a Lenovo Thinkpad T460, Intel Core i5-6300U, 16GB RAM, Release Mode x64 w/ optimization,
no debugger.

| TestName                       | ElapsedTime      | Remarks                                                                                                       | MemoryUsage | Result |
|--------------------------------|------------------|---------------------------------------------------------------------------------------------------------------|-------------|--------|
| TestSingleCollectionProperty() | 00:00:02.2809203 | This is only a timing reference. See method documentation for details.                                        | 84,1 MB     |        |
| TestUsingIndividualDbSetLoad() | 00:00:02.6549794 | Flags: 0                                                                                                      | 101 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.8895760 | Flags: AutoDetectChangesEnabled                                                                               | 93,9 MB     | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.8314755 | Flags: LazyLoadingEnabled                                                                                     | 103 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.7527288 | Flags: AutoDetectChangesEnabled, LazyLoadingEnabled                                                           | 112 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.8531002 | Flags: UseDatabaseNullSemantics                                                                               | 106 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.7963131 | Flags: AutoDetectChangesEnabled, UseDatabaseNullSemantics                                                     | 105 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.7492397 | Flags: LazyLoadingEnabled, UseDatabaseNullSemantics                                                           | 105 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.7095967 | Flags: AutoDetectChangesEnabled, LazyLoadingEnabled, UseDatabaseNullSemantics                                 | 93,0 MB     | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.6157835 | Flags: ProxyCreationEnabled                                                                                   | 109 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.5993611 | Flags: AutoDetectChangesEnabled, ProxyCreationEnabled                                                         | 103 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.9004617 | Flags: LazyLoadingEnabled, ProxyCreationEnabled                                                               | 106 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.7671333 | Flags: AutoDetectChangesEnabled, LazyLoadingEnabled, ProxyCreationEnabled                                     | 94,8 MB     | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.8797741 | Flags: UseDatabaseNullSemantics, ProxyCreationEnabled                                                         | 112 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.8186107 | Flags: AutoDetectChangesEnabled, UseDatabaseNullSemantics, ProxyCreationEnabled                               | 105 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.6573565 | Flags: LazyLoadingEnabled, UseDatabaseNullSemantics, ProxyCreationEnabled                                     | 103 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:02.9807240 | Flags: AutoDetectChangesEnabled, LazyLoadingEnabled, UseDatabaseNullSemantics, ProxyCreationEnabled           | 106 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:12.6192447 | Flags: UseLocal                                                                                               | 111 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:14.4685267 | Flags: AutoDetectChangesEnabled, UseLocal                                                                     | 114 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:13.5810211 | Flags: LazyLoadingEnabled, UseLocal                                                                           | 97,9 MB     | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:14.3622859 | Flags: AutoDetectChangesEnabled, LazyLoadingEnabled, UseLocal                                                 | 103 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:12.9208761 | Flags: UseDatabaseNullSemantics, UseLocal                                                                     | 124 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:14.3734035 | Flags: AutoDetectChangesEnabled, UseDatabaseNullSemantics, UseLocal                                           | 100 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:12.1470335 | Flags: LazyLoadingEnabled, UseDatabaseNullSemantics, UseLocal                                                 | 105 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:13.2232086 | Flags: AutoDetectChangesEnabled, LazyLoadingEnabled, UseDatabaseNullSemantics, UseLocal                       | 108 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:12.7090454 | Flags: ProxyCreationEnabled, UseLocal                                                                         | 110 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:12.9587466 | Flags: AutoDetectChangesEnabled, ProxyCreationEnabled, UseLocal                                               | 112 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:12.1527329 | Flags: LazyLoadingEnabled, ProxyCreationEnabled, UseLocal                                                     | 104 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:13.8463033 | Flags: AutoDetectChangesEnabled, LazyLoadingEnabled, ProxyCreationEnabled, UseLocal                           | 110 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:13.6456567 | Flags: UseDatabaseNullSemantics, ProxyCreationEnabled, UseLocal                                               | 111 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:12.7665244 | Flags: AutoDetectChangesEnabled, UseDatabaseNullSemantics, ProxyCreationEnabled, UseLocal                     | 110 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:12.2727132 | Flags: LazyLoadingEnabled, UseDatabaseNullSemantics, ProxyCreationEnabled, UseLocal                           | 103 MB      | ok     |
| TestUsingIndividualDbSetLoad() | 00:00:13.5972568 | Flags: AutoDetectChangesEnabled, LazyLoadingEnabled, UseDatabaseNullSemantics, ProxyCreationEnabled, UseLocal | 114 MB      | ok     |

### Note about the memory usage
Note that the memory usage may or may not be accurate, but it should be useful
for comparison. Currently, the sqlite database is about 20MB in size, so you can (very roughly) compare the
effective in-memory usage for models yourself. Note that cached data or EF6 tracking collections might still be
in place when the memory consumption is measured - don't consider the values shown as realistic - only compare
them against each other!


## Automatic indexes required for many-to-many collections

Due to the way EF6 creates the join queries it's actually required to rely on automatic indexes generated by
SQLite. I do not know of any way to apply an index to a join from multiple tables. This especially weights
heavy if you wanted to, for example, load presets individually for each plugin. This is also the reason why such
a benchmark was not implemented - it is simply *way* too slow.

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

You should avoid using `DbSet<>.Local`, as this will always create an `ObservableCollection` and comes with quite a big
performance penalty. See the 


# Motivation

I develop [PresetMagician](https://presetmagician.com), a specialized application for format conversion between
[VST Plugins](https://en.wikipedia.org/wiki/Virtual_Studio_Technology) and the
[NKS](https://www.native-instruments.com/en/specials/komplete/this-is-nks/) format. The application
is a single-user, single-instance frontend application which required a persistence layer at some point. I thought
quite a long time about if I'd use an embedded SQL database or some other persistence layer. My requirements are:

- Minimal or no data conversion between what's stored in the persistence layer and the application model hierarchy
- Plays well with [Catel](https://www.catelproject.com/), especially with the
  [ModelBase](http://docs.catelproject.com/5.8/catel-core/data-handling/modelbase/) as well as the built-in
  [IEditableObject](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.ieditableobject) support 
- The application is data heavy.
  - As you can see in the included sqlite3 example database (which is my personal
    test/development database), the data sets are quite large. It's not uncommon that one plugin alone has several 1000s
    of presets, and it's not uncommon that a user has several 100s of plugins installed.
  - Most of the data needs to be available at all times, be it for the user's editing purposes or for the conversion
    process itself. As such, the entity model is designed to be as memory friendly as possible. The currently only heavy
    data is the preset data itself, which is kept in a separate table and only loaded when it's actually needed and only
    saved if plugin presets are inserted/updated.
  - Note that the included database does NOT contain the actual preset data, since the actual preset data (stored in 
    `PresetDataStorages`) is copyrighted by the respective author(s) and/or vendor(s).
- Editing must be quick.
  - Applying a set of `Types` (which is [Native Instruments](https://www.native-instruments.com) speech for
    "Instrument Type") and `Modes` (which describes the preset's techniques or how it sounds) like to many 100s of presets
    should not require the user to wait seconds.
  - Changing the `BankPath` (which defines the logical categories of a preset, like **Felicia's Presets > Favourites** 
    is mapped to an Entity tree on database load for easier editing) should happen near instant, even if the user drags
    and drops 1000s of presets at once.
  - At any point during the editing process, the user needs to be able to cancel editing in case of mistakes and
    (hopefully) soon be able to undo individual actions.
  - `Types` or `Modes` are stored globally, because it makes no sense to store the `Type` **Drums > Kick** in several
    places - this also allows the user to quickly consolidate similar data like **Drums > Kick**, **Drums > Bass Drum**,
    **Drums > Kicks** etc into a single type, across all presets of all plugins, including raising
    `INotifyPropertyChanged` and updating all views without the need to write additional code
  - On the other hand, performance is not *that* critical if it involves specialized code in many places, or even
    firing SQL statements manually.
  - Basically this means: Development must be as fluent as possible, without even noticing that the persistence layer is
    there (which, in contrast, means: the application must in theory work without a persistence layer)

Of course, I could use a [NoSQL](https://en.wikipedia.org/wiki/NoSQL) database. However, I personally have zero
experience with NoSQL databases. In contrast, I do have 20+ years experience with relational databases and I'm aware
of most caveats. Also, I personally feel that a good database design also helps with the object model design itself.

As such, my requirements were an ORM which supports:
 1. Entity property to Database Column mapping
 2. Persisting changes (and ONLY changes) in an easy way
 3. Keep the models clean from database "quirks" (like an additional join table for m:n relations)
 4. Easy to implement and use
 
EntityFramework6 fulfills all criteria, and #3 is the reason why I'm not using EntityFramework.Core yet. However, the
bad performance of EF6 in some regards raised the need to do research, and that's why this repository is here.

The benchmark above basically does research on how to load all required data as quickly as possible. For change tracking
I'm currently evaluating [TrackableEntities](https://github.com/TrackableEntities/trackable-entities), but I guess
that I'll develop a custom, slim layer which does change tracking, as this will most likely be required for undo/redo
functionality anyways. After all, I only need to attach changed entities to EF6 to get data persisted (I hope).