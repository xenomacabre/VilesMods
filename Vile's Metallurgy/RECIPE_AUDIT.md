# Recipe audit

Last reviewed: 2026-07-26

Scope: all 78 concrete `RecipeDef` entries in `Defs/Recipes`.

## Ingredient-filter rules

- An ingredient slot's `<filter>` defines what can satisfy that slot.
- `<fixedIngredientFilter>` is the recipe-wide hard limit used by bills. Every interchangeable candidate must also be allowed here.
- `<defaultIngredientFilter>` only defines the bill's initial selection. It cannot permit anything excluded by the fixed filter.
- A slot with exactly one possible `ThingDef` is a fixed ingredient. It may be omitted from `fixedIngredientFilter` to keep it out of the bill filter GUI while remaining required by the recipe.

## Corrections made

- `Make_ComponentIndustrialCast`: allowed `SandResource` and `SoftClay` in `fixedIngredientFilter`. The molding-material slot has two alternatives, so both must pass the recipe-wide filter.
- `Make_PipeSectionBasic_Forged`: replaced the invalid `ThingCategoryDef` reference to `Metallic` with `stuffCategoriesToAllow`. `Metallic` is a `StuffCategoryDef`.
- `wootz_process` and `wootzPigIron_process`: removed the stale `ClayPot` entries from `fixedIngredientFilter`; the actual fixed ingredient is `ClayPotWetUnglazed`.
- `MakePigIron_Furnace`: removed `Coal` and `Metallurgical` from the fixed filter because the corresponding ingredient slot is commented out and the furnace consumes fuel through its fuel component.
- `MakeBronze_Hand` and `MakeBronze_Foundry`: removed the unused raw `Copper` entry; both recipes consume `CopperBar`.
- Corrected product-count text for:
  - `MakeForgeWeldedSteel_Smithy` (15)
  - `MakeForgeWeldedSteel_Finery` (15)
  - `MakeWroughtIron_PigIron_Finery` (30)
  - `MakeForgeWeldedSteel_BlisterSteel_Finery` (30)
  - `MakeBronze_Hand` (25)
  - `MakeCopperBars_Foundry` (50)
  - `MakeTinBars_Foundry` (50)
  - `MakeLeadBars_Hand` (15)
  - `MakeLeadBars_Foundry` (25 lead bars, plus 20 pure silver when Materials Science is active)

## Intentional fixed ingredients

`MakeCrucibleSteel` requires `SoftClay`, but it is intentionally absent from `fixedIngredientFilter`: its ingredient slot has no alternative, so it stays required without appearing as a configurable bill-filter option.

After the corrections above, no other empty ingredient/filter intersections or unresolved explicit ingredient, product, or recipe-user `ThingDef` references were found in this recipe set.
