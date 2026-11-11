// Copyright (c) 2026 Neil Colvin. MIT with Commons Clause; see LICENSE.
// Favorites grid additions (c) 2026 contributors to the yoelkam/AppleTVCrestronDriver fork.
// The slot properties/commands below are repetitive by design: Crestron Home extension UIs bind
// each button to its own flat property, so every slot needs its own label/icon/visible values.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

using Crestron.DeviceDrivers.EntityModel.Data;
using Crestron.DeviceDrivers.SDK.EntityModel.Attributes;

namespace AppleTV.CrestronDriver.Extension;

/// <content>
/// Favorites grid: up to <see cref="AppleTvFavorites.SLOT_COUNT"/> one-tap app buttons on the
/// main page, filled automatically from popular installed apps until the installer customizes
/// them on the "Edit favorites" page. Custom assignments are saved per Apple TV and survive
/// reboots. See <see cref="AppleTvFavorites"/> for the pure selection logic.
/// </content>
public sealed partial class AppleTvExtensionDriver
	{
	private const string FAVORITES_STORE_BASE_MODEL = "Apple TV Companion Media";
	private static readonly object _favoritesFileLock = new ();
	private string[] _customFavorites;
	private readonly string[] _favoriteBundleIds = new string[AppleTvFavorites.SLOT_COUNT];

	#region Favorites properties

	[EntityProperty (Id = "fav1Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav1Label
		{
		get;
		private set => SetAndNotify ("fav1Label", value, ref field);
		}
	[EntityProperty (Id = "fav1Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav1Icon
		{
		get;
		private set => SetAndNotify ("fav1Icon", value, ref field);
		}
	[EntityProperty (Id = "fav1Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav1Visible
		{
		get;
		private set => SetAndNotify ("fav1Visible", value, ref field);
		}
	[EntityProperty (Id = "fav1EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav1EditLabel
		{
		get;
		private set => SetAndNotify ("fav1EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav2Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav2Label
		{
		get;
		private set => SetAndNotify ("fav2Label", value, ref field);
		}
	[EntityProperty (Id = "fav2Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav2Icon
		{
		get;
		private set => SetAndNotify ("fav2Icon", value, ref field);
		}
	[EntityProperty (Id = "fav2Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav2Visible
		{
		get;
		private set => SetAndNotify ("fav2Visible", value, ref field);
		}
	[EntityProperty (Id = "fav2EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav2EditLabel
		{
		get;
		private set => SetAndNotify ("fav2EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav3Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav3Label
		{
		get;
		private set => SetAndNotify ("fav3Label", value, ref field);
		}
	[EntityProperty (Id = "fav3Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav3Icon
		{
		get;
		private set => SetAndNotify ("fav3Icon", value, ref field);
		}
	[EntityProperty (Id = "fav3Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav3Visible
		{
		get;
		private set => SetAndNotify ("fav3Visible", value, ref field);
		}
	[EntityProperty (Id = "fav3EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav3EditLabel
		{
		get;
		private set => SetAndNotify ("fav3EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav4Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav4Label
		{
		get;
		private set => SetAndNotify ("fav4Label", value, ref field);
		}
	[EntityProperty (Id = "fav4Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav4Icon
		{
		get;
		private set => SetAndNotify ("fav4Icon", value, ref field);
		}
	[EntityProperty (Id = "fav4Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav4Visible
		{
		get;
		private set => SetAndNotify ("fav4Visible", value, ref field);
		}
	[EntityProperty (Id = "fav4EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav4EditLabel
		{
		get;
		private set => SetAndNotify ("fav4EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav5Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav5Label
		{
		get;
		private set => SetAndNotify ("fav5Label", value, ref field);
		}
	[EntityProperty (Id = "fav5Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav5Icon
		{
		get;
		private set => SetAndNotify ("fav5Icon", value, ref field);
		}
	[EntityProperty (Id = "fav5Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav5Visible
		{
		get;
		private set => SetAndNotify ("fav5Visible", value, ref field);
		}
	[EntityProperty (Id = "fav5EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav5EditLabel
		{
		get;
		private set => SetAndNotify ("fav5EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav6Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav6Label
		{
		get;
		private set => SetAndNotify ("fav6Label", value, ref field);
		}
	[EntityProperty (Id = "fav6Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav6Icon
		{
		get;
		private set => SetAndNotify ("fav6Icon", value, ref field);
		}
	[EntityProperty (Id = "fav6Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav6Visible
		{
		get;
		private set => SetAndNotify ("fav6Visible", value, ref field);
		}
	[EntityProperty (Id = "fav6EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav6EditLabel
		{
		get;
		private set => SetAndNotify ("fav6EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav7Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav7Label
		{
		get;
		private set => SetAndNotify ("fav7Label", value, ref field);
		}
	[EntityProperty (Id = "fav7Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav7Icon
		{
		get;
		private set => SetAndNotify ("fav7Icon", value, ref field);
		}
	[EntityProperty (Id = "fav7Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav7Visible
		{
		get;
		private set => SetAndNotify ("fav7Visible", value, ref field);
		}
	[EntityProperty (Id = "fav7EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav7EditLabel
		{
		get;
		private set => SetAndNotify ("fav7EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav8Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav8Label
		{
		get;
		private set => SetAndNotify ("fav8Label", value, ref field);
		}
	[EntityProperty (Id = "fav8Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav8Icon
		{
		get;
		private set => SetAndNotify ("fav8Icon", value, ref field);
		}
	[EntityProperty (Id = "fav8Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav8Visible
		{
		get;
		private set => SetAndNotify ("fav8Visible", value, ref field);
		}
	[EntityProperty (Id = "fav8EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav8EditLabel
		{
		get;
		private set => SetAndNotify ("fav8EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav9Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav9Label
		{
		get;
		private set => SetAndNotify ("fav9Label", value, ref field);
		}
	[EntityProperty (Id = "fav9Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav9Icon
		{
		get;
		private set => SetAndNotify ("fav9Icon", value, ref field);
		}
	[EntityProperty (Id = "fav9Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav9Visible
		{
		get;
		private set => SetAndNotify ("fav9Visible", value, ref field);
		}
	[EntityProperty (Id = "fav9EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav9EditLabel
		{
		get;
		private set => SetAndNotify ("fav9EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav10Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav10Label
		{
		get;
		private set => SetAndNotify ("fav10Label", value, ref field);
		}
	[EntityProperty (Id = "fav10Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav10Icon
		{
		get;
		private set => SetAndNotify ("fav10Icon", value, ref field);
		}
	[EntityProperty (Id = "fav10Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav10Visible
		{
		get;
		private set => SetAndNotify ("fav10Visible", value, ref field);
		}
	[EntityProperty (Id = "fav10EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav10EditLabel
		{
		get;
		private set => SetAndNotify ("fav10EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav11Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav11Label
		{
		get;
		private set => SetAndNotify ("fav11Label", value, ref field);
		}
	[EntityProperty (Id = "fav11Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav11Icon
		{
		get;
		private set => SetAndNotify ("fav11Icon", value, ref field);
		}
	[EntityProperty (Id = "fav11Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav11Visible
		{
		get;
		private set => SetAndNotify ("fav11Visible", value, ref field);
		}
	[EntityProperty (Id = "fav11EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav11EditLabel
		{
		get;
		private set => SetAndNotify ("fav11EditLabel", value, ref field);
		}
	[EntityProperty (Id = "fav12Label", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav12Label
		{
		get;
		private set => SetAndNotify ("fav12Label", value, ref field);
		}
	[EntityProperty (Id = "fav12Icon", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav12Icon
		{
		get;
		private set => SetAndNotify ("fav12Icon", value, ref field);
		}
	[EntityProperty (Id = "fav12Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool Fav12Visible
		{
		get;
		private set => SetAndNotify ("fav12Visible", value, ref field);
		}
	[EntityProperty (Id = "fav12EditLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string Fav12EditLabel
		{
		get;
		private set => SetAndNotify ("fav12EditLabel", value, ref field);
		}
	[EntityProperty (Id = "favRow1Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool FavRow1Visible
		{
		get;
		private set => SetAndNotify ("favRow1Visible", value, ref field);
		}
	[EntityProperty (Id = "favRow2Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool FavRow2Visible
		{
		get;
		private set => SetAndNotify ("favRow2Visible", value, ref field);
		}
	[EntityProperty (Id = "favRow3Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool FavRow3Visible
		{
		get;
		private set => SetAndNotify ("favRow3Visible", value, ref field);
		}
	[EntityProperty (Id = "favRow4Visible")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool FavRow4Visible
		{
		get;
		private set => SetAndNotify ("favRow4Visible", value, ref field);
		}

	[EntityProperty (Id = "favoritesEmpty")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public bool FavoritesEmpty
		{
		get;
		private set => SetAndNotify ("favoritesEmpty", value, ref field);
		}

	[EntityProperty (Id = "favoritesModeLabel", Type = DriverEntityValueType.String)]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public string FavoritesModeLabel
		{
		get;
		private set => SetAndNotify ("favoritesModeLabel", value, ref field);
		}

	// Source list for the editor's per-slot pickers: "(Empty)" followed by every installed app.
	[EntityProperty (Id = "favoriteChoiceList")]
	[EntityPropertyMetadata (ExtensionUiProperty = true)]
	public AppleTvAppItem[] FavoriteChoiceList
		{
		get;
		private set => SetAndNotify ("favoriteChoiceList", value, ref field);
		}

	#endregion Favorites properties

	#region Favorites commands

	/// <summary>Launches the app in favorites slot 1.</summary>
	[EntityCommand (Id = "launchFavorite1")]
	public void LaunchFavorite1 () => LaunchFavoriteSlot (0);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 1 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite1")]
	public void AssignFavorite1 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (0, value);
	/// <summary>Launches the app in favorites slot 2.</summary>
	[EntityCommand (Id = "launchFavorite2")]
	public void LaunchFavorite2 () => LaunchFavoriteSlot (1);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 2 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite2")]
	public void AssignFavorite2 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (1, value);
	/// <summary>Launches the app in favorites slot 3.</summary>
	[EntityCommand (Id = "launchFavorite3")]
	public void LaunchFavorite3 () => LaunchFavoriteSlot (2);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 3 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite3")]
	public void AssignFavorite3 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (2, value);
	/// <summary>Launches the app in favorites slot 4.</summary>
	[EntityCommand (Id = "launchFavorite4")]
	public void LaunchFavorite4 () => LaunchFavoriteSlot (3);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 4 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite4")]
	public void AssignFavorite4 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (3, value);
	/// <summary>Launches the app in favorites slot 5.</summary>
	[EntityCommand (Id = "launchFavorite5")]
	public void LaunchFavorite5 () => LaunchFavoriteSlot (4);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 5 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite5")]
	public void AssignFavorite5 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (4, value);
	/// <summary>Launches the app in favorites slot 6.</summary>
	[EntityCommand (Id = "launchFavorite6")]
	public void LaunchFavorite6 () => LaunchFavoriteSlot (5);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 6 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite6")]
	public void AssignFavorite6 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (5, value);
	/// <summary>Launches the app in favorites slot 7.</summary>
	[EntityCommand (Id = "launchFavorite7")]
	public void LaunchFavorite7 () => LaunchFavoriteSlot (6);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 7 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite7")]
	public void AssignFavorite7 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (6, value);
	/// <summary>Launches the app in favorites slot 8.</summary>
	[EntityCommand (Id = "launchFavorite8")]
	public void LaunchFavorite8 () => LaunchFavoriteSlot (7);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 8 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite8")]
	public void AssignFavorite8 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (7, value);
	/// <summary>Launches the app in favorites slot 9.</summary>
	[EntityCommand (Id = "launchFavorite9")]
	public void LaunchFavorite9 () => LaunchFavoriteSlot (8);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 9 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite9")]
	public void AssignFavorite9 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (8, value);
	/// <summary>Launches the app in favorites slot 10.</summary>
	[EntityCommand (Id = "launchFavorite10")]
	public void LaunchFavorite10 () => LaunchFavoriteSlot (9);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 10 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite10")]
	public void AssignFavorite10 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (9, value);
	/// <summary>Launches the app in favorites slot 11.</summary>
	[EntityCommand (Id = "launchFavorite11")]
	public void LaunchFavorite11 () => LaunchFavoriteSlot (10);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 11 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite11")]
	public void AssignFavorite11 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (10, value);
	/// <summary>Launches the app in favorites slot 12.</summary>
	[EntityCommand (Id = "launchFavorite12")]
	public void LaunchFavorite12 () => LaunchFavoriteSlot (11);

	/// <summary>Assigns an app (by bundle ID) to favorites slot 12 from the editor list.</summary>
	[EntityCommand (Id = "assignFavorite12")]
	public void AssignFavorite12 ([EntityParameter (Id = "value", Type = DriverEntityValueType.String)] string value) => AssignFavoriteSlot (11, value);

	/// <summary>Discards custom favorites and returns to the automatic popular-apps selection.</summary>
	[EntityCommand (Id = "resetFavorites")]
	public void ResetFavorites ()
		{
		lock (_stateLock)
			{
			_customFavorites = null;
			}

		DeleteSavedFavorites (_lastUniqueId);
		RefreshFavorites ();
		}

	#endregion Favorites commands

	private void InitializeFavorites ()
		{
		FavoriteChoiceList = [new AppleTvAppItem (AppleTvFavorites.EMPTY_SLOT_ID, "(Empty)")];
		RefreshFavorites ();
		}

	private void LoadFavoritesFor (string uniqueId)
		{
		string[] saved = LoadSavedFavorites (uniqueId);
		lock (_stateLock)
			{
			_customFavorites = saved;
			}

		RefreshFavorites ();
		}

	private void LaunchFavoriteSlot (int slotIndex)
		{
		string bundleId;
		lock (_stateLock)
			{
			bundleId = _favoriteBundleIds[slotIndex];
			}

		if (string.IsNullOrEmpty (bundleId))
			{
			LogWarning ($"Favorites slot {slotIndex + 1} is empty.");
			return;
			}

		_ = LaunchSelectedAppAsync (bundleId);
		}

	private void AssignFavoriteSlot (int slotIndex, string bundleId)
		{
		string[] updated;
		string uniqueId;
		lock (_stateLock)
			{
			// Customizing starts from what's on screen now, so the first edit doesn't wipe the
			// automatic picks in the other slots.
			string[] current = _customFavorites ?? (string[])_favoriteBundleIds.Clone ();
			updated = AppleTvFavorites.Assign (current, slotIndex, bundleId);
			_customFavorites = updated;
			uniqueId = _lastUniqueId;
			}

		SaveFavorites (uniqueId, updated);
		RefreshFavorites ();
		}

	/// <summary>Recomputes every slot from the current app list and custom assignments.</summary>
	private void RefreshFavorites ()
		{
		List<(string BundleId, string Name)> apps;
		string[] custom;
		lock (_stateLock)
			{
			apps = [.. _apps];
			custom = _customFavorites;
			}

		(string BundleId, string Name)[] slots = AppleTvFavorites.Resolve (apps, custom);
		int filled = 0;
		for (int index = 0; index < AppleTvFavorites.SLOT_COUNT; index++)
			{
			(string bundleId, string name) = slots[index];
			bool visible = !string.IsNullOrEmpty (bundleId);
			lock (_stateLock)
				{
				_favoriteBundleIds[index] = visible ? bundleId : null;
				}

			if (visible)
				filled++;

			string editLabel = $"{index + 1}. " + (visible ? name : (custom != null && index < custom.Length && !string.IsNullOrEmpty (custom[index]) ? "(Not installed)" : "(Empty)"));
			ApplyFavoriteSlot (index, visible ? name : string.Empty, visible ? AppleTvFavorites.GetIcon (bundleId, name) : "icPlaceholder", visible, editLabel);
			}

		for (int row = 0; row < AppleTvFavorites.SLOT_COUNT / AppleTvFavorites.SLOTS_PER_ROW; row++)
			{
			bool rowVisible = false;
			for (int column = 0; column < AppleTvFavorites.SLOTS_PER_ROW; column++)
				rowVisible |= !string.IsNullOrEmpty (slots[row * AppleTvFavorites.SLOTS_PER_ROW + column].BundleId);
			ApplyFavoriteRow (row, rowVisible);
			}

		FavoritesEmpty = filled == 0;
		FavoritesModeLabel = custom == null ? "Automatic (popular apps)" : "Custom";

		var choices = new AppleTvAppItem[apps.Count + 1];
		choices[0] = new AppleTvAppItem (AppleTvFavorites.EMPTY_SLOT_ID, "(Empty)");
		for (int index = 0; index < apps.Count; index++)
			choices[index + 1] = new AppleTvAppItem (apps[index].BundleId, apps[index].Name);
		FavoriteChoiceList = choices;
		}

	private void ApplyFavoriteSlot (int index, string label, string icon, bool visible, string editLabel)
		{
		switch (index)
			{
			case 0: Fav1Label = label; Fav1Icon = icon; Fav1Visible = visible; Fav1EditLabel = editLabel; break;
			case 1: Fav2Label = label; Fav2Icon = icon; Fav2Visible = visible; Fav2EditLabel = editLabel; break;
			case 2: Fav3Label = label; Fav3Icon = icon; Fav3Visible = visible; Fav3EditLabel = editLabel; break;
			case 3: Fav4Label = label; Fav4Icon = icon; Fav4Visible = visible; Fav4EditLabel = editLabel; break;
			case 4: Fav5Label = label; Fav5Icon = icon; Fav5Visible = visible; Fav5EditLabel = editLabel; break;
			case 5: Fav6Label = label; Fav6Icon = icon; Fav6Visible = visible; Fav6EditLabel = editLabel; break;
			case 6: Fav7Label = label; Fav7Icon = icon; Fav7Visible = visible; Fav7EditLabel = editLabel; break;
			case 7: Fav8Label = label; Fav8Icon = icon; Fav8Visible = visible; Fav8EditLabel = editLabel; break;
			case 8: Fav9Label = label; Fav9Icon = icon; Fav9Visible = visible; Fav9EditLabel = editLabel; break;
			case 9: Fav10Label = label; Fav10Icon = icon; Fav10Visible = visible; Fav10EditLabel = editLabel; break;
			case 10: Fav11Label = label; Fav11Icon = icon; Fav11Visible = visible; Fav11EditLabel = editLabel; break;
			case 11: Fav12Label = label; Fav12Icon = icon; Fav12Visible = visible; Fav12EditLabel = editLabel; break;
			}
		}

	private void ApplyFavoriteRow (int row, bool visible)
		{
		switch (row)
			{
			case 0: FavRow1Visible = visible; break;
			case 1: FavRow2Visible = visible; break;
			case 2: FavRow3Visible = visible; break;
			case 3: FavRow4Visible = visible; break;
			}
		}

	private static string GetFavoritesEntryId (string uniqueId)
		{
		char[] characters = ("favorites-" + uniqueId.Trim ().ToUpperInvariant ()).ToCharArray ();
		char[] invalid = Path.GetInvalidFileNameChars ();
		for (int index = 0; index < characters.Length; index++)
			if (Array.IndexOf (invalid, characters[index]) >= 0)
				characters[index] = '_';
		return new string (characters) + ".json";
		}

	private string[] LoadSavedFavorites (string uniqueId)
		{
		if (string.IsNullOrWhiteSpace (uniqueId))
			return null;

		try
			{
			lock (_favoritesFileLock)
				{
				var store = new CrestronCredentialFileStore (FAVORITES_STORE_BASE_MODEL);
				string entryId = GetFavoritesEntryId (uniqueId);
				if (!store.EnumerateEntries ().Contains (entryId, StringComparer.OrdinalIgnoreCase))
					return null;

				using Stream stream = store.OpenRead (entryId);
				var lines = (string[])new DataContractJsonSerializer (typeof (string[])).ReadObject (stream);
				return lines is null || lines.Length == 0 ? null : AppleTvFavorites.Deserialize (string.Join ("\n", lines));
				}
			}
		catch (Exception exception)
			{
			LogWarning ("Could not load saved favorites; using automatic favorites. " + exception.Message);
			return null;
			}
		}

	private void SaveFavorites (string uniqueId, string[] slots)
		{
		if (string.IsNullOrWhiteSpace (uniqueId))
			{
			LogWarning ("Favorites changed before an Apple TV was configured; they won't be saved.");
			return;
			}

		try
			{
			lock (_favoritesFileLock)
				{
				string[] lines = AppleTvFavorites.Serialize (slots).Split ('\n');
				using var buffer = new MemoryStream ();
				new DataContractJsonSerializer (typeof (string[])).WriteObject (buffer, lines);
				new CrestronCredentialFileStore (FAVORITES_STORE_BASE_MODEL).WriteAtomically (GetFavoritesEntryId (uniqueId), buffer.ToArray ());
				}
			}
		catch (Exception exception)
			{
			LogError ("Could not save favorites: " + exception.Message);
			}
		}

	private void DeleteSavedFavorites (string uniqueId)
		{
		if (string.IsNullOrWhiteSpace (uniqueId))
			return;

		// Writing an empty list is treated as "no custom favorites" by LoadSavedFavorites, and
		// avoids needing a delete operation on the shared file store.
		try
			{
			lock (_favoritesFileLock)
				{
				using var buffer = new MemoryStream ();
				new DataContractJsonSerializer (typeof (string[])).WriteObject (buffer, Array.Empty<string> ());
				new CrestronCredentialFileStore (FAVORITES_STORE_BASE_MODEL).WriteAtomically (GetFavoritesEntryId (uniqueId), buffer.ToArray ());
				}
			}
		catch (Exception exception)
			{
			LogError ("Could not reset saved favorites: " + exception.Message);
			}
		}
	}
