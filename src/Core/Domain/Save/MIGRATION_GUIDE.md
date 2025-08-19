# Save System Migration Guide

## Overview
The BlockLife save system implements automatic version migration to ensure backward compatibility. When loading older save files, the system automatically applies migrations to upgrade the data to the current format.

## Creating a New Migration

When you need to change the save format (adding fields, changing structure, etc.), follow these steps:

### 1. Increment the Version
In `SaveData.cs`, increment the `CURRENT_VERSION` constant:
```csharp
public const int CURRENT_VERSION = 2; // Was 1
```

### 2. Create a Migration Class
Create a new migration in `src/Core/Infrastructure/Services/Migrations/`:

```csharp
public sealed class V1ToV2Migration : SaveMigrationBase
{
    public override int FromVersion => 1;
    public override int ToVersion => 2;
    public override string Description => "Add new feature X to save format";

    public override Fin<SaveData> Migrate(SaveData data)
    {
        // Transform the data from version 1 to version 2
        var migrated = data with
        {
            Version = ToVersion,
            // Add new fields with defaults
            NewField = GetDefaultValue(),
            // Transform existing fields if needed
            ExistingField = TransformField(data.ExistingField)
        };

        return FinSucc(migrated);
    }
}
```

### 3. Register the Migration
In `GameStrapper.cs`, add your migration to the DI container:

```csharp
// --- Save System with Versioning (TD_015) ---
services.AddTransient<ISaveMigration, ExampleV0ToV1Migration>();
services.AddTransient<ISaveMigration, V1ToV2Migration>(); // Add your new migration
```

### 4. Write Tests
Add tests for your migration in `SaveServiceTests.cs`:

```csharp
[Fact]
public void Migration_FromV1ToV2_ShouldAddNewFields()
{
    // Arrange
    var migration = new V1ToV2Migration();
    var v1Data = new SaveData { Version = 1, /* ... */ };

    // Act
    var result = migration.Migrate(v1Data);

    // Assert
    result.IsSucc.Should().BeTrue();
    result.Match(
        Succ: data =>
        {
            data.Version.Should().Be(2);
            data.NewField.Should().NotBeNull();
            // Verify transformations
        },
        Fail: _ => throw new Exception("Migration should succeed")
    );
}
```

## Migration Best Practices

### DO:
- ✅ Always preserve existing data during migration
- ✅ Provide sensible defaults for new fields
- ✅ Document what each migration does
- ✅ Test migrations with real save files
- ✅ Chain migrations properly (0→1→2→3)
- ✅ Use immutable transformations (with expressions)

### DON'T:
- ❌ Delete or lose user data during migration
- ❌ Skip version numbers (must be sequential)
- ❌ Modify the original SaveData parameter
- ❌ Throw exceptions (use Fin<T> for errors)
- ❌ Make assumptions about field existence

## Common Migration Scenarios

### Adding a New Field
```csharp
var migrated = data with
{
    Version = ToVersion,
    NewField = "default value"
};
```

### Renaming a Field
```csharp
var migrated = data with
{
    Version = ToVersion,
    NewFieldName = data.OldFieldName,
    OldFieldName = null // Clear old field
};
```

### Transforming Data Structure
```csharp
var migrated = data with
{
    Version = ToVersion,
    // Convert list to dictionary
    BlocksById = data.Blocks.ToDictionary(b => b.Id, b => b)
};
```

### Handling Missing Data
```csharp
public override Fin<SaveData> Migrate(SaveData data)
{
    // Check for required data
    if (data.CriticalField == null)
    {
        return FinFail<SaveData>(
            MigrationError("Cannot migrate: missing critical field"));
    }
    
    // Proceed with migration
    var migrated = data with { /* ... */ };
    return FinSucc(migrated);
}
```

## Testing Migrations

### Manual Testing
1. Create a save file in the old version
2. Update to new version with migration
3. Load the save file
4. Verify all data is preserved and transformed correctly

### Automated Testing
The `SaveServiceTests` includes comprehensive migration tests:
- Version detection
- Migration chaining
- Error handling
- Data preservation

## Troubleshooting

### "No migration path" Error
- Ensure migrations are registered in DI container
- Check that version numbers are sequential
- Verify CanMigrate returns true for the version

### Data Loss After Migration
- Check that all fields are copied in the migration
- Ensure you're using `with` expressions correctly
- Add logging to track migration steps

### Migration Performance
- Migrations run synchronously during load
- Keep transformations simple and efficient
- Consider async operations for large data sets

## Version History

| Version | Date | Description |
|---------|------|-------------|
| 0 | Pre-release | Unversioned saves (legacy) |
| 1 | 2025-08-19 | Initial versioning system |
| 2 | Future | (Reserved for next migration) |