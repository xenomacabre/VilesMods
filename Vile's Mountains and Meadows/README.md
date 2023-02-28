# About BiomesKit
Note! BiomesKit does not add anything new on it's own. It's intended to be used by other modders to more easily add new Biomes.

BiomesKit is a framework for the placement of biomes on the world map at worldgen. Previously, modders needed to utilize C# to add new biomes to rimworld. With BiomesKit, placement of a biome can be controlled with XML tags in the BiomeDef itself.

With BiomesKit as a dependency, you too can make your own biomes mod with minimal hassle! Features of BiomesKit includes but are not limited to:
* Spawning biomes based on factors like elevation, rainfall, hilliness and more!
* Restricting your biome to spawn only on specific vanilla biomes, or even biomes from other mods!
* Changing the hilliness of a tile to a specific level, or even randomizing it!
* Custom world map tile graphics for your biome!

BiomesKit works by adding tags in the BiomeDef through a modExtension. For example, here's how I did it on the tropical island for Biomes!
```xml
<li Class="BiomesKit.BiomesKitControls">
      <randomizeHilliness>True</randomizeHilliness>
      <usePerlin>True</usePerlin>
      <perlinCulling>0.5</perlinCulling>
      <allowOnWater>True</allowOnWater>
      <minTemperature>15</minTemperature>
      <minRainfall>600</minRainfall>
      <frequency>5</frequency>
      <materialPath>World/MapGraphics/TropicalIsland</materialPath>
      <materialLayer>3509</materialLayer>
</li>
```
[BiomesKit manual](https://steamcommunity.com/linkfilter/?url=https://github.com/biomes-team/BiomesKit/wiki)

### Credits to:
- Oskar Potocki for the BiomesKit logo
- Everyone in #Mod-Development on the rimworld server for helping me learn C#
