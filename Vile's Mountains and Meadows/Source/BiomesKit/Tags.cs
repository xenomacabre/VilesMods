using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BiomesKit
{

	public class BiomesKitControls : DefModExtension
	{
		public List<BiomeDef> spawnOnBiomes = new List<BiomeDef>();
		public int? biomePriority;
		public string materialPath = "World/MapGraphics/Default";
		public bool forested = false;
		public bool uniqueHills = false;
		public float forestSnowyBelow = -9999;
		public float forestSparseBelow = -9999;
		public float forestDenseAbove = 9999;
		public int materialLayer = 3515;
		public float smallHillSizeMultiplier = 1.5f;
		public float largeHillSizeMultiplier = 2f;
		public float mountainSizeMultiplier = 1.4f;
		public float impassableSizeMultiplier = 1.3f;
		public float materialSizeMultiplier = 1;
		public float materialOffset = 1f;
		public bool materialRandomRotation = true;
		public Hilliness materialMinHilliness;
		public Hilliness materialMaxHilliness;
		public bool allowOnWater = false;
		public bool allowOnLand = true;
		public bool setNotWaterCovered = false;
		public int minimumWaterNeighbors = 0;
		public int minimumLandNeighbors = 0;
		public bool needRiver = false;
		public bool randomizeHilliness = false;
		public float minTemperature = -999;
		public float maxTemperature = 999;
		public float minElevation = -9999;
		public float maxElevation = 9999;
		public float? setElevation;
		public float minNorthLatitude = -9999;
		public float maxNorthLatitude = -9999;
		public float minSouthLatitude = -9999;
		public float maxSouthLatitude = -9999;
		public Hilliness minHilliness = Hilliness.Flat;
		public Hilliness maxHilliness = Hilliness.Impassable;
		public BiomesKitHilliness? spawnHills = null;
		public BiomesKitHilliness? setHills = null;
		public BiomesKitHilliness? minRandomHills = BiomesKitHilliness.Flat;
		public BiomesKitHilliness? maxRandomHills = BiomesKitHilliness.Mountainous;
		public float snowpilesBelow = -9999;
		public float mountainsSemiSnowyBelow = -9999;
		public float mountainsSnowyBelow = -9999;
		public float mountainsVerySnowyBelow = -9999;
		public float mountainsFullySnowyBelow = -9999;
		public float impassableSemiSnowyBelow = -9999;
		public float impassableSnowyBelow = -9999;
		public float impassableVerySnowyBelow = -9999;
		public float impassableFullySnowyBelow = -9999;
		public float minRainfall = -9999;
		public float maxRainfall = 9999;
		public int frequency = 100;
		public bool useAlternativePerlinSeedPreset = false;
		public bool usePerlin = false;
		public int? perlinCustomSeed = null;
		public float perlinCulling = 0.99f;
		public double perlinFrequency;
		public double perlinLacunarity;
		public double perlinPersistence;
		public int perlinOctaves;
	}
}
