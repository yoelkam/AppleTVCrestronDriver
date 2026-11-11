// Copyright (c) 2026 Neil Colvin. MIT with Commons Clause; see LICENSE.
// Favorites grid additions (c) 2026 contributors to the yoelkam/AppleTVCrestronDriver fork.

using System;
using System.Collections.Generic;
using System.Linq;

namespace AppleTV.CrestronDriver.Extension;

/// <summary>
/// Pure, side-effect-free logic behind the favorites grid on the extension's main page: which
/// installed apps fill the fixed button slots, in what order, and which Crestron Home library
/// icon each one shows. Kept free of Crestron types so it can be unit tested directly.
/// </summary>
/// <remarks>
/// Two modes. <b>Automatic</b> (no saved list): the grid is filled from <see cref="PopularApps"/>
/// in that order, using only apps actually installed on the Apple TV, then topped up
/// alphabetically. <b>Custom</b> (a saved list exists): the installer's chosen bundle IDs, in
/// slot order; a slot whose app is no longer installed shows as empty rather than shifting the
/// other favorites around.
/// </remarks>
public static class AppleTvFavorites
	{
	/// <summary>Number of favorite buttons on the main page (4 rows of 3).</summary>
	public const int SLOT_COUNT = 12;

	/// <summary>Number of favorite buttons per grid row.</summary>
	public const int SLOTS_PER_ROW = 3;

	/// <summary>Sentinel used by the editor's "(Empty)" list item to clear a slot.</summary>
	public const string EMPTY_SLOT_ID = "__empty__";

	/// <summary>
	/// Well-known apps used to seed the automatic favorites, in priority order. Matched against
	/// installed apps by bundle ID first, then by exact display name (case-insensitive), so a
	/// regional or renamed bundle still matches by its name.
	/// </summary>
	public static readonly IReadOnlyList<(string BundleId, string Name)> PopularApps =
		[
		("com.netflix.Netflix", "Netflix"),
		("com.google.ios.youtube", "YouTube"),
		("com.disney.disneyplus", "Disney+"),
		("com.apple.TVWatchList", "TV"),
		("com.amazon.aiv.AIVApp", "Prime Video"),
		("com.wbd.stream", "Max"),
		("com.hulu.plus", "Hulu"),
		("com.google.ios.youtubeunplugged", "YouTube TV"),
		("com.peacocktv.peacock", "Peacock"),
		("com.cbsvideo.app", "Paramount+"),
		("com.spotify.client", "Spotify"),
		("com.apple.TVMusic", "Music"),
		("com.plexapp.plex", "Plex"),
		("com.espn.ScoreCenter", "ESPN"),
		];

	/// <summary>
	/// Resolves the apps shown in each slot. Returns exactly <see cref="SLOT_COUNT"/> entries;
	/// empty slots are <c>(null, null)</c>.
	/// </summary>
	/// <param name="installedApps">Apps currently installed on the Apple TV.</param>
	/// <param name="customSlots">
	/// Saved custom slot assignments (bundle ID or null/empty per slot), or <see langword="null"/>
	/// for automatic mode.
	/// </param>
	public static (string BundleId, string Name)[] Resolve (
		IReadOnlyList<(string BundleId, string Name)> installedApps,
		IReadOnlyList<string> customSlots)
		{
		installedApps ??= [];
		var slots = new (string BundleId, string Name)[SLOT_COUNT];

		if (customSlots != null)
			{
			for (int index = 0; index < SLOT_COUNT && index < customSlots.Count; index++)
				{
				string bundleId = customSlots[index];
				if (string.IsNullOrEmpty (bundleId))
					{
					continue;
					}

				(string BundleId, string Name) match = installedApps.FirstOrDefault (app => string.Equals (app.BundleId, bundleId, StringComparison.Ordinal));
				if (match.BundleId != null)
					{
					slots[index] = match;
					}
				}

			return slots;
			}

		var chosen = new List<(string BundleId, string Name)> ();
		foreach ((string popularBundleId, string popularName) in PopularApps)
			{
			(string BundleId, string Name) match = installedApps.FirstOrDefault (app => string.Equals (app.BundleId, popularBundleId, StringComparison.Ordinal));
			if (match.BundleId == null)
				{
				match = installedApps.FirstOrDefault (app => string.Equals (app.Name, popularName, StringComparison.OrdinalIgnoreCase));
				}

			if (match.BundleId != null && !chosen.Any (app => app.BundleId == match.BundleId))
				{
				chosen.Add (match);
				}
			}

		foreach ((string BundleId, string Name) app in installedApps.OrderBy (app => app.Name, StringComparer.CurrentCultureIgnoreCase))
			{
			if (chosen.Count >= SLOT_COUNT)
				{
				break;
				}

			if (!chosen.Any (existing => existing.BundleId == app.BundleId) && !IsSystemUtility (app))
				{
				chosen.Add (app);
				}
			}

		for (int index = 0; index < SLOT_COUNT && index < chosen.Count; index++)
			{
			slots[index] = chosen[index];
			}

		return slots;
		}

	/// <summary>
	/// Returns a copy of <paramref name="currentSlots"/> (bundle IDs per slot) with
	/// <paramref name="slotIndex"/> set to <paramref name="bundleId"/>. Assigning an app that is
	/// already in another slot swaps the two, so a favorite never appears twice and the
	/// installer can reorder by reassigning. <see cref="EMPTY_SLOT_ID"/> clears the slot.
	/// </summary>
	public static string[] Assign (IReadOnlyList<string> currentSlots, int slotIndex, string bundleId)
		{
		if (slotIndex < 0 || slotIndex >= SLOT_COUNT)
			{
			throw new ArgumentOutOfRangeException (nameof (slotIndex));
			}

		var slots = new string[SLOT_COUNT];
		for (int index = 0; index < SLOT_COUNT; index++)
			{
			slots[index] = currentSlots != null && index < currentSlots.Count ? currentSlots[index] ?? string.Empty : string.Empty;
			}

		string newValue = string.IsNullOrEmpty (bundleId) || bundleId == EMPTY_SLOT_ID ? string.Empty : bundleId;
		if (newValue.Length > 0)
			{
			int existingIndex = Array.IndexOf (slots, newValue);
			if (existingIndex >= 0 && existingIndex != slotIndex)
				{
				slots[existingIndex] = slots[slotIndex];
				}
			}

		slots[slotIndex] = newValue;
		return slots;
		}

	/// <summary>
	/// Picks a Crestron Home library icon (name without the leading '#') that suggests the app's
	/// category. App logos cannot be shown in Crestron Home extension UIs, so this is the closest
	/// visual cue available.
	/// </summary>
	public static string GetIcon (string bundleId, string name)
		{
		string key = ((bundleId ?? string.Empty) + " " + (name ?? string.Empty)).ToLowerInvariant ();

		if (ContainsAny (key, "com.apple.tvwatchlist") || string.Equals (name, "TV", StringComparison.OrdinalIgnoreCase))
			return "icTvOn";
		if (ContainsAny (key, "youtube tv", "youtubeunplugged", "espn", "sling", "fubo", "directv", "news", "cnn", "fox sports", "nbc sports", "live tv", "pluto"))
			return "icBroadcastRegular";
		if (ContainsAny (key, "spotify", "music", "pandora", "tidal", "deezer", "sirius", "iheart", "radio", "sonos", "qobuz"))
			return "icAudioOn";
		if (ContainsAny (key, "podcast"))
			return "icMicrophone";
		if (ContainsAny (key, "arcade", "game", "steam", "xbox", "geforce"))
			return "icGames";
		if (ContainsAny (key, "fitness", "peloton", "workout", "yoga"))
			return "icFitness";
		if (ContainsAny (key, "photo", "camera", "unifi protect"))
			return "icCamera";
		if (ContainsAny (key, "facetime", "zoom", "webex"))
			return "icMeetingRoom";
		if (ContainsAny (key, "settings"))
			return "icSettings";
		if (ContainsAny (key, "search"))
			return "icSearch";
		if (ContainsAny (key, "youtube", "twitch", "vimeo"))
			return "icPlayCircled";

		return "icPlayingVideoRegular";
		}

	/// <summary>Serializes slot assignments for storage (one bundle ID per line).</summary>
	public static string Serialize (IReadOnlyList<string> slots) =>
		string.Join ("\n", Enumerable.Range (0, SLOT_COUNT).Select (index => slots != null && index < slots.Count ? slots[index] ?? string.Empty : string.Empty));

	/// <summary>Parses stored slot assignments; returns <see langword="null"/> for empty/invalid content.</summary>
	public static string[] Deserialize (string content)
		{
		if (string.IsNullOrWhiteSpace (content))
			{
			return null;
			}

		string[] lines = content.Replace ("\r", string.Empty).Split ('\n');
		var slots = new string[SLOT_COUNT];
		for (int index = 0; index < SLOT_COUNT; index++)
			{
			slots[index] = index < lines.Length ? lines[index].Trim () : string.Empty;
			}

		return slots;
		}

	private static bool IsSystemUtility ((string BundleId, string Name) app) =>
		app.BundleId != null && (app.BundleId.StartsWith ("com.apple.TVSettings", StringComparison.Ordinal)
			|| app.BundleId.StartsWith ("com.apple.TVSearch", StringComparison.Ordinal)
			|| app.BundleId.StartsWith ("com.apple.TVAppStore", StringComparison.Ordinal)
			|| app.BundleId.StartsWith ("com.apple.TVPhotos", StringComparison.Ordinal)
			|| app.BundleId.StartsWith ("com.apple.Fitness", StringComparison.Ordinal)
			|| app.BundleId.StartsWith ("com.apple.facetime", StringComparison.OrdinalIgnoreCase));

	private static bool ContainsAny (string text, params string[] needles) => needles.Any (needle => text.Contains (needle));
	}
