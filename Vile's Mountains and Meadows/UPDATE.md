# Update Instructions

When making changes to the mod that you're going to release, make sure to take these steps.

- Update the `AssemblyInfo.cs` file's `AssemblyVersion` amd `AssemblyFileVersion`. Change the right most number up by one if it's a minor fix. The second right-most number if there's a new feature. The second left-most digit if there's significant changes (reworking something). The left-most number, maybe move up only if there's a significant game version change involved.
- Build the assembly
- Install the mod above other mods and test that it loads in game (try doing some world generation with the BiomesCore mod)
- Copy the `1.1/Assemblies/BiomesKit.dll` into `BiomesCore/1.1/Assemblies/`
- When releasing release the BiomesKit first ahead of BiomesCore changes.

This will keep both mods in sync and avoid user warning with older DLL loads.
