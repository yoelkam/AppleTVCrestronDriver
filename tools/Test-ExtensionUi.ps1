# Validates the extension driver's UiDefinition.xml the way Crestron's
# "Crestron Extension Driver AI Skill" (Crestron/CrestronAISkills, crestron-driver-extension v1.4.2)
# says to: against Crestron's published XSD, plus the skill's list of common errors
# (BOM, empty attributes, invalid binding syntax, duplicate IDs, default-layout rules),
# plus cross-checks that every bound property/command/translation exists in the driver.
param(
    [string] $ExtensionDir = (Join-Path $PSScriptRoot '..\AppleTVCrestronExtensionDriver'),
    [string] $XsdUrl = 'https://prd-use-rad-assets.azurewebsites.net/ExtensionsSchemaDefinition.xsd'
)
$ErrorActionPreference = 'Stop'
$errors = [System.Collections.Generic.List[string]]::new()
$uiPath = Join-Path $ExtensionDir 'IncludeInPkg\UiDefinitions\UiDefinition.xml'
$trPath = Join-Path $ExtensionDir 'IncludeInPkg\Translations\en-US.json'

# 1. No BOM (skill: "UTF-8 only, NO BOM").
$bytes = [System.IO.File]::ReadAllBytes($uiPath)
if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { $errors.Add('UiDefinition.xml starts with a UTF-8 BOM.') }

# 2. Crestron XSD validation.
$xsdPath = Join-Path ([System.IO.Path]::GetTempPath()) 'ExtensionsSchemaDefinition.xsd'
Invoke-WebRequest -Uri $XsdUrl -OutFile $xsdPath -UseBasicParsing
$settings = [System.Xml.XmlReaderSettings]::new()
$settings.ValidationType = [System.Xml.ValidationType]::Schema
$settings.ValidationFlags = $settings.ValidationFlags -bor [System.Xml.Schema.XmlSchemaValidationFlags]::ReportValidationWarnings
$null = $settings.Schemas.Add($null, $xsdPath)
$settings.add_ValidationEventHandler({ param($s, $e) $errors.Add("XSD ($($e.Severity)) line $($e.Exception.LineNumber): $($e.Message)") })
$reader = [System.Xml.XmlReader]::Create($uiPath, $settings)
try { while ($reader.Read()) { } } finally { $reader.Close() }
Write-Host "XSD validation done against $XsdUrl"

# 3. Skill's common-error checks.
[xml] $doc = Get-Content -LiteralPath $uiPath -Raw
$all = $doc.SelectNodes('//*')
$ids = @()
foreach ($node in $all) {
    foreach ($attr in $node.Attributes) {
        if ($attr.Name -like 'xmlns*' -or $attr.Name -like 'xsi:*') { continue }
        if ([string]::IsNullOrWhiteSpace($attr.Value)) { $errors.Add("Empty attribute '$($attr.Name)' on <$($node.Name)>.") }
        $v = $attr.Value
        $ok = $v -match '^(\{[A-Za-z0-9_:]+\}|\{\.[A-Za-z0-9_]*\}|\{\.\}|\^[A-Za-z0-9_]+(\|[^|]+)*|#[A-Za-z0-9]+|\$[A-Za-z0-9_.]+|command:[A-Za-z0-9_]+|show:[A-Za-z0-9_]+|showalert:[A-Za-z0-9_]+|back|[A-Za-z0-9_.]+)$'
        if ($attr.Name -ne 'ver' -and -not $ok) { $errors.Add("Suspicious binding syntax: $($node.Name)@$($attr.Name)='$v'") }
        if ($attr.Name -eq 'id') { $ids += $v }
    }
}
$ids | Group-Object | Where-Object Count -gt 1 | ForEach-Object { $errors.Add("Duplicate id '$($_.Name)'.") }
$defaults = @($doc.SelectNodes('//layout[@isdefaultlayout="#true"]'))
if ($defaults.Count -ne 1) { $errors.Add("Expected exactly one default layout, found $($defaults.Count).") }
foreach ($d in $defaults) { if ($d.HasAttribute('inputparameters')) { $errors.Add("Default layout '$($d.id)' must not have inputparameters.") } }
foreach ($g in $doc.SelectNodes('//buttongroup')) { if ($g.ChildNodes.Count -gt 5) { $errors.Add('A buttongroup has more than 5 buttons.') } }
foreach ($g in $doc.SelectNodes('//controlgroup')) { if ($g.ChildNodes.Count -lt 2) { $errors.Add('A controlgroup has fewer than 2 controls.') } }
$layoutIds = @($doc.SelectNodes('//layout') | ForEach-Object { $_.id })
$alertIds = @($doc.SelectNodes('//alerts/*') | ForEach-Object { $_.id })

# 4. Cross-check bindings against the driver source and translations.
$source = (Get-ChildItem -LiteralPath $ExtensionDir -Filter *.cs | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw }) -join "`n"
$props = [regex]::Matches($source, 'EntityProperty \(Id = "([\w:]+)"') | ForEach-Object { $_.Groups[1].Value }
$commands = [regex]::Matches($source, 'EntityCommand \(Id = "(\w+)"') | ForEach-Object { $_.Groups[1].Value }
$translations = (Get-Content -LiteralPath $trPath -Raw | ConvertFrom-Json).PSObject.Properties.Name
$raw = Get-Content -LiteralPath $uiPath -Raw
foreach ($m in [regex]::Matches($raw, '\{([A-Za-z0-9_]+)\}')) { if ($props -notcontains $m.Groups[1].Value) { $errors.Add("Bound property '{$($m.Groups[1].Value)}' is not declared in the driver.") } }
foreach ($m in [regex]::Matches($raw, 'command:(\w+)')) { if ($commands -notcontains $m.Groups[1].Value) { $errors.Add("Command '$($m.Groups[1].Value)' is not declared in the driver.") } }
foreach ($m in [regex]::Matches($raw, '\^(\w+)')) { if ($translations -notcontains $m.Groups[1].Value) { $errors.Add("Translation key '^$($m.Groups[1].Value)' is missing from en-US.json.") } }
foreach ($m in [regex]::Matches($raw, 'show:(\w+)')) { if ($layoutIds -notcontains $m.Groups[1].Value) { $errors.Add("Navigation target layout '$($m.Groups[1].Value)' does not exist.") } }
foreach ($m in [regex]::Matches($raw, 'showalert:(\w+)')) { if ($alertIds -notcontains $m.Groups[1].Value) { $errors.Add("Alert '$($m.Groups[1].Value)' does not exist.") } }

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Host "::error::$_" }
    throw "Extension UI validation failed with $($errors.Count) problem(s)."
}
Write-Host 'Extension UI validation passed: XSD, BOM, attributes, bindings, IDs, layouts, properties, commands, translations.'
