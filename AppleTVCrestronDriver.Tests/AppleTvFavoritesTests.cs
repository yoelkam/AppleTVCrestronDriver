// Copyright (c) 2026 Neil Colvin. MIT with Commons Clause; see LICENSE.
// Favorites grid tests (c) 2026 contributors to the yoelkam/AppleTVCrestronDriver fork.

using System.Collections.Generic;
using System.Linq;

using AppleTV.CrestronDriver.Extension;

using NUnit.Framework;
using NUnit.Framework.Legacy;

using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AppleTVCrestronDriver.Tests;

[TestFixture]
public sealed class AppleTvFavoritesTests
	{
	private static readonly List<(string BundleId, string Name)> Installed =
		[
		("com.apple.TVSettings", "Settings"),
		("com.spotify.client", "Spotify"),
		("com.example.zeta", "Zeta"),
		("com.google.ios.youtube", "YouTube"),
		("com.netflix.Netflix", "Netflix"),
		("com.example.alpha", "Alpha"),
		("com.regional.disney", "Disney+"),
		];

	[Test]
	public void Resolve_Automatic_OrdersPopularAppsFirstAndMatchesByName ()
		{
		(string BundleId, string Name)[] slots = AppleTvFavorites.Resolve (Installed, null);

		Assert.AreEqual (AppleTvFavorites.SLOT_COUNT, slots.Length);
		Assert.AreEqual ("Netflix", slots[0].Name);
		Assert.AreEqual ("YouTube", slots[1].Name);
		Assert.AreEqual ("com.regional.disney", slots[2].BundleId, "Disney+ should match by display name when the bundle ID differs.");
		Assert.AreEqual ("Spotify", slots[3].Name);
		Assert.AreEqual ("Alpha", slots[4].Name, "Remaining slots are topped up alphabetically.");
		Assert.AreEqual ("Zeta", slots[5].Name);
		Assert.IsNull (slots[6].BundleId, "System utilities such as Settings are not auto-added.");
		}

	[Test]
	public void Resolve_Automatic_EmptyAppList_ReturnsAllEmptySlots ()
		{
		(string BundleId, string Name)[] slots = AppleTvFavorites.Resolve ([], null);

		Assert.AreEqual (AppleTvFavorites.SLOT_COUNT, slots.Length);
		Assert.IsTrue (slots.All (slot => slot.BundleId == null));
		}

	[Test]
	public void Resolve_Custom_KeepsSlotPositionsAndSkipsUninstalledApps ()
		{
		string[] custom = new string[AppleTvFavorites.SLOT_COUNT];
		custom[0] = "com.example.zeta";
		custom[1] = "com.removed.app";
		custom[4] = "com.netflix.Netflix";

		(string BundleId, string Name)[] slots = AppleTvFavorites.Resolve (Installed, custom);

		Assert.AreEqual ("Zeta", slots[0].Name);
		Assert.IsNull (slots[1].BundleId, "An uninstalled favorite leaves its slot empty instead of shifting the others.");
		Assert.IsNull (slots[2].BundleId);
		Assert.AreEqual ("Netflix", slots[4].Name);
		}

	[Test]
	public void Assign_SetsSlot ()
		{
		string[] result = AppleTvFavorites.Assign (null, 2, "com.netflix.Netflix");

		Assert.AreEqual (AppleTvFavorites.SLOT_COUNT, result.Length);
		Assert.AreEqual ("com.netflix.Netflix", result[2]);
		Assert.AreEqual (string.Empty, result[0]);
		}

	[Test]
	public void Assign_AppAlreadyInAnotherSlot_SwapsInsteadOfDuplicating ()
		{
		string[] current = AppleTvFavorites.Assign (AppleTvFavorites.Assign (null, 0, "a"), 1, "b");

		string[] result = AppleTvFavorites.Assign (current, 0, "b");

		Assert.AreEqual ("b", result[0]);
		Assert.AreEqual ("a", result[1]);
		Assert.AreEqual (1, result.Count (id => id == "b"));
		}

	[Test]
	public void Assign_EmptySentinel_ClearsSlot ()
		{
		string[] current = AppleTvFavorites.Assign (null, 3, "a");

		string[] result = AppleTvFavorites.Assign (current, 3, AppleTvFavorites.EMPTY_SLOT_ID);

		Assert.AreEqual (string.Empty, result[3]);
		}

	[Test]
	public void Assign_OutOfRangeSlot_Throws ()
		{
		_ = NUnit.Framework.Assert.Throws<System.ArgumentOutOfRangeException> (() => AppleTvFavorites.Assign (null, AppleTvFavorites.SLOT_COUNT, "a"));
		}

	[Test]
	public void SerializeDeserialize_RoundTripsSlots ()
		{
		string[] slots = AppleTvFavorites.Assign (AppleTvFavorites.Assign (null, 0, "com.a"), 11, "com.b");

		string[] restored = AppleTvFavorites.Deserialize (AppleTvFavorites.Serialize (slots));

		CollectionAssert.AreEqual (slots, restored);
		}

	[Test]
	public void Deserialize_Empty_ReturnsNullForAutomaticMode ()
		{
		Assert.IsNull (AppleTvFavorites.Deserialize (string.Empty));
		Assert.IsNull (AppleTvFavorites.Deserialize (null));
		}

	[TestCase ("com.apple.TVWatchList", "TV", "icTvOn")]
	[TestCase ("com.google.ios.youtubeunplugged", "YouTube TV", "icBroadcastRegular")]
	[TestCase ("com.google.ios.youtube", "YouTube", "icPlayCircled")]
	[TestCase ("com.spotify.client", "Spotify", "icAudioOn")]
	[TestCase ("com.apple.TVMusic", "Music", "icAudioOn")]
	[TestCase ("com.netflix.Netflix", "Netflix", "icPlayingVideoRegular")]
	[TestCase ("com.paramount.app", "Paramount+", "icPlayingVideoRegular")]
	public void GetIcon_PicksCategoryIcon (string bundleId, string name, string expected)
		{
		Assert.AreEqual (expected, AppleTvFavorites.GetIcon (bundleId, name));
		}
	}
