using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable once CheckNamespace
namespace NestedNavigationProperties.Models.Ef6
{
public class Mode
{
    [Key] public int Id { get; set; }

    public ICollection<Plugin> Plugins { get; set; }
    public ICollection<Preset> Presets { get; set; }

    public string Name { get; set; }
}

public class Type
{
    [Key] public int Id { get; set; }

    public ICollection<Plugin> Plugins { get; set; } = new List<Plugin>();
    public ICollection<Preset> Presets { get; set; } = new List<Preset>();

    public string Name { get; set; }
    public string SubTypeName { get; set; }
}

public class Plugin
{
    [Key] public int Id { get; set; }
    public ICollection<Preset> Presets { get; set; } = new List<Preset>();

    public ICollection<Type> DefaultTypes { get; set; } = new List<Type>();
    public ICollection<Mode> DefaultModes { get; set; } = new List<Mode>();
}

public class Preset
{
    [Key] public string PresetId { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey("Plugin")] public int PluginId { get; set; }

    public Plugin Plugin { get; set; }
    public ICollection<Type> Types { get; set; } = new List<Type>();
    public ICollection<Mode> Modes { get; set; } = new List<Mode>();
}
}