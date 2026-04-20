# Analyza problemu s prazdnym `CCU_260409_2.2.9623`

## Zisteny stav
- Vystup projektu `Cgp.NCAS.CCU` sa realne vytvori a zabalenie prebehne do suboru `C:\w2OriginCodex\nova\Cgp.NCAS.CCU\bin\CCU_260409_2.2.9623.gz`.
- Problem nie je v tom, ze by sa nevytvoril balicek. Problem je, ze novy balicek neobsahuje adresar `CCUUpgrader`, ktory v starsom balicku bol.
- Porovnanie rozbalenych balikov:
  - stary `CCU_260330_2.2.9623.gz` obsahuje `CCUUpgrader\CCUUpgrader.exe`, `Version.txt` a dalsie DLL
  - novy `CCU_260409_2.2.9623.gz` adresar `CCUUpgrader` neobsahuje vobec
- Velkost balikov tomu zodpoveda:
  - stary: `1852762 B`
  - novy: `1472451 B`
  - rozdiel: `380311 B`

## Kde je pricina v kode
V subore `C:\w2OriginCodex\nova\Cgp.NCAS.CCU\Cgp.NCAS.CCU.csproj` je toto:

```xml
<ItemGroup>
  <CcuUpgraderBinaries Include="$(SolutionDir)Cgp.NCAS.CCUUpgrader\bin\$(ConfigurationName)\*.exe" />
  <CcuUpgraderBinaries Include="$(SolutionDir)Cgp.NCAS.CCUUpgrader\bin\$(ConfigurationName)\*.txt" />
  <CcuUpgraderBinaries Include="$(SolutionDir)Cgp.NCAS.CCUUpgrader\bin\$(ConfigurationName)\*.dll" />
</ItemGroup>
<Target Name="AfterBuild">
  <Copy SourceFiles="@(CcuUpgraderBinaries)" DestinationFolder="$(TargetDir)CCUUpgrader\">
```

Podstatny problem je, ze `@(CcuUpgraderBinaries)` sa definujú **mimo** targetu `AfterBuild`.
To znamena, ze wildcard `...\bin\Release\*.exe/*.dll/*.txt` sa vyhodnoti pri nacitani projektu, nie az v momente ked sa `AfterBuild` vykonava.

## Preco sa to prejavilo az na novom pocitaci
Toto sedi presne na spravanie po cistom stiahnuti branchu alebo po clean workspace:
- pri prvom builde este v `Cgp.NCAS.CCUUpgrader\bin\Release` nic nie je
- projekt `Cgp.NCAS.CCU` si preto nacita prazdny zoznam `@(CcuUpgraderBinaries)`
- neskor sa sice `Cgp.NCAS.CCUUpgrader` v tom istom builde skompiluje, ale `@(CcuUpgraderBinaries)` sa uz neprepocita
- `AfterBuild` teda nema co kopirovat do `bin\Release\CCUUpgrader`
- `FilePackerTool.exe` potom zabalí `bin\Release` bez priecinka `CCUUpgrader`

Na starsom PC to mohlo "fungovat" len preto, ze v `Cgp.NCAS.CCUUpgrader\bin\Release` uz zostali subory z nejakeho predchadzajuceho buildu. Vtedy wildcard pri nacitani projektu nasiel existujuce subory a kopirovanie vyslo.

## Dalsie overenie
- Solution `CGP-NCAS 2.2 CF35.sln` ma sice zavislost `Cgp.NCAS.CCU -> Cgp.NCAS.CCUUpgrader`, cize poradie buildu je nastavene.
- To ale nestaci, pretoze chyba nie je v poradi buildu, ale v casovani vyhodnotenia wildcard itemov v `.csproj`.

## K tym varovaniam
Varovania typu `ResolveAssemblyReference.cache` alebo `GenerateResource.Cache` s tymto problemom priamo nesuvisia. Su to sprievodne build warningy. Na prazdny obsah medzikroku / chybanie `CCUUpgrader` v `.gz` je rozhodujuci prave sposob definicie `@(CcuUpgraderBinaries)`.

## Najpravdepodobnejsi zaver
Nejde o zmenu branchu. Je to stara krehkost build procesu, ktora sa prejavi na cistom prostredi. Novy pocitac len odstranil stare build artefakty, na ktorych tento postup ticho zavisel.

## Odporucany fix
Najcistejsie riesenie je presunut definiciu `CcuUpgraderBinaries` **do targetu `AfterBuild`**, aby sa wildcard vyhodnotil az po dobuildeni upgraderu. Alternativne spravit z `Cgp.NCAS.CCUUpgrader` realny `ProjectReference` alebo explicitny build/copy krok, ktory sa vykona az po jeho zostaveni.
