using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
#if KEEPIDEO_UI
using UnityEngine;
#endif

namespace KeepIdeo
{
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Settings â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public class KeepIdeoSettings : ModSettings
    {
        public bool roles = true, rituals = true, buildings = true, relics = true,
                    weapons = true, animals = true, xenotypes = true, apparel = true,
                    other = true, appearance = true, styles = true;
        public bool keepNames = true;
        public bool addRequiredFromNewMemes = true;
        public bool dropInvalidForNewMemes = false;
        public bool skipInMultiplayer = true;
        public bool verboseLog = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref roles, "roles", true);
            Scribe_Values.Look(ref rituals, "rituals", true);
            Scribe_Values.Look(ref buildings, "buildings", true);
            Scribe_Values.Look(ref relics, "relics", true);
            Scribe_Values.Look(ref weapons, "weapons", true);
            Scribe_Values.Look(ref animals, "animals", true);
            Scribe_Values.Look(ref xenotypes, "xenotypes", true);
            Scribe_Values.Look(ref apparel, "apparel", true);
            Scribe_Values.Look(ref other, "other_v2", true);
            Scribe_Values.Look(ref appearance, "appearance", true);
            Scribe_Values.Look(ref styles, "styles_v2", true);
            Scribe_Values.Look(ref keepNames, "keepNames_v2", true);
            Scribe_Values.Look(ref addRequiredFromNewMemes, "addRequiredFromNewMemes", true);
            Scribe_Values.Look(ref dropInvalidForNewMemes, "dropInvalid_v2", false);
            Scribe_Values.Look(ref skipInMultiplayer, "skipInMultiplayer", true);
            Scribe_Values.Look(ref verboseLog, "verboseLog_v2", false);
        }

        public bool Enabled(Cat c)
        {
            switch (c)
            {
                case Cat.Role: return roles;
                case Cat.Ritual: return rituals;
                case Cat.Building: return buildings;
                case Cat.Relic: return relics;
                case Cat.Weapon: return weapons;
                case Cat.Animal: return animals;
                case Cat.Xenotype: return xenotypes;
                case Cat.Apparel: return apparel;
                case Cat.Other: return other;
                default: return false;
            }
        }
    }

    public class KeepIdeoMod : Mod
    {
        public static KeepIdeoSettings Settings;

        public KeepIdeoMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<KeepIdeoSettings>();
            var harmony = new Harmony("keepideo.memechange");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            NameGuard.PatchRegenerateName(harmony);
        }

#if KEEPIDEO_UI
        public override string SettingsCategory() => "Keep Ideoligion Details on Meme Change";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var l = new Listing_Standard();
            l.Begin(inRect);
            l.Label("Keep these when memes change:");
            l.CheckboxLabeled("Roles", ref Settings.roles);
            l.CheckboxLabeled("Rituals", ref Settings.rituals);
            l.CheckboxLabeled("Buildings (altars, ideograms, etc.)", ref Settings.buildings);
            l.CheckboxLabeled("Relics", ref Settings.relics);
            l.CheckboxLabeled("Preferred / despised weapons", ref Settings.weapons);
            l.CheckboxLabeled("Venerated animals", ref Settings.animals);
            l.CheckboxLabeled("Preferred xenotypes", ref Settings.xenotypes);
            l.CheckboxLabeled("Required apparel", ref Settings.apparel);
            l.CheckboxLabeled("Other (modded) special precepts", ref Settings.other);
            l.CheckboxLabeled("Appearance (hair, beard, tattoo frequencies)", ref Settings.appearance);
            l.CheckboxLabeled("Style categories (affects building looks)", ref Settings.styles);
            l.CheckboxLabeled("Keep names of roles, rituals, buildings and relics", ref Settings.keepNames);
            l.GapLine();
            l.CheckboxLabeled("Add precepts required by newly added memes", ref Settings.addRequiredFromNewMemes);
            l.CheckboxLabeled("Drop kept precepts that need a removed meme", ref Settings.dropInvalidForNewMemes);
            l.CheckboxLabeled("Don't run during multiplayer games (avoids desyncs)", ref Settings.skipInMultiplayer);
            l.CheckboxLabeled("Detailed log (lists everything kept / removed)", ref Settings.verboseLog);
            l.GapLine();
            l.Label("Tip: hold Shift when accepting the meme dialog to get the vanilla full reroll.");
            l.End();
        }
#endif
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Categories â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public enum Cat { None, Role, Ritual, Building, Relic, Weapon, Animal, Xenotype, Apparel, Other }

    internal static class Classifier
    {
        public static Cat Classify(Precept p)
        {
            if (p == null) return Cat.None;
            for (Type t = p.GetType(); t != null && t != typeof(Precept); t = t.BaseType)
            {
                switch (t.Name)
                {
                    case "Precept_Role": return Cat.Role;
                    case "Precept_Ritual": return Cat.Ritual;
                    case "Precept_Building": return Cat.Building;
                    case "Precept_Relic": return Cat.Relic;
                    case "Precept_Weapon": return Cat.Weapon;
                    case "Precept_Animal": return Cat.Animal;
                    case "Precept_Xenotype": return Cat.Xenotype;
                    case "Precept_Apparel": return Cat.Apparel;
                }
            }
            // Plain Precept = ordinary meme-driven stance (left to vanilla); any other subclass = modded special precept
            return p.GetType() != typeof(Precept) ? Cat.Other : Cat.None;
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Reflection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    internal static class Refl
    {
        public static readonly FieldInfo IdeoStyle = AccessTools.Field(typeof(Ideo), "style");
        public static readonly FieldInfo ThingStyleCategories = AccessTools.Field(typeof(Ideo), "thingStyleCategories");
        public static readonly FieldInfo PreceptName = AccessTools.Field(typeof(Precept), "name");
        public static readonly MethodInfo RecachePrecepts = Find(typeof(Ideo), "RecachePrecepts", 0);
        public static readonly MethodInfo SortStyleCategories = Find(typeof(Ideo), "SortStyleCategories", 0);

        public static readonly MethodInfo GetFrequency;
        public static readonly MethodInfo SetFrequency;
        public static readonly FieldInfo StyleForThingDef;

        public static readonly FieldInfo DialogIdeo;
        public static readonly FieldInfo DialogInitial;

        static Refl()
        {
            Type styleType = IdeoStyle?.FieldType;
            if (styleType != null)
            {
                GetFrequency = Find(styleType, "GetFrequency", 1);
                SetFrequency = Find(styleType, "SetFrequency", 2);
                StyleForThingDef = AccessTools.Field(styleType, "styleForThingDef");
            }

            var dlgFields = AccessTools.GetDeclaredFields(typeof(Dialog_ChooseMemes));
            DialogIdeo = dlgFields.FirstOrDefault(f => !f.IsStatic && f.FieldType == typeof(Ideo));
            DialogInitial = dlgFields.FirstOrDefault(f => !f.IsStatic && f.FieldType == typeof(bool)
                                                          && f.Name.ToLowerInvariant().Contains("initial"));

            if (GetFrequency == null || SetFrequency == null)
                Log.Warning("[KeepIdeo] Couldn't find style frequency methods; appearance won't be preserved.");
            if (DialogIdeo == null)
                Log.Warning("[KeepIdeo] Couldn't find Dialog_ChooseMemes.ideo field; will try constructor args.");
        }

        private static MethodInfo Find(Type t, string name, int paramCount) =>
            AccessTools.GetDeclaredMethods(t)
                .FirstOrDefault(m => m.Name == name && !m.IsStatic && m.GetParameters().Length == paramCount);

        public static string Describe(Precept p)
        {
            string n = null;
            try { n = PreceptName?.GetValue(p) as string; } catch { }
            return (p?.def?.defName ?? "?") + (string.IsNullOrEmpty(n) ? "" : " \"" + n + "\"");
        }

        public static void V(string msg)
        {
            if (KeepIdeoMod.Settings.verboseLog || Prefs.DevMode) Log.Message("[KeepIdeo] " + msg);
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Full per-precept snapshot â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    internal class PreceptState
    {
        private static readonly Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();

        public readonly Precept precept;
        private readonly FieldInfo[] fields;
        private readonly object[] values;
        private readonly List<object>[] listItems;
        private readonly List<KeyValuePair<object, object>>[] dictItems;

        public PreceptState(Precept p)
        {
            precept = p;
            fields = FieldsOf(p.GetType());
            values = new object[fields.Length];
            listItems = new List<object>[fields.Length];
            dictItems = new List<KeyValuePair<object, object>>[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                object v;
                try { v = fields[i].GetValue(p); } catch { continue; }
                values[i] = v;
                if (v is IList l && !(v is Array))
                    listItems[i] = l.Cast<object>().ToList();
                else if (v is IDictionary d)
                    dictItems[i] = d.Keys.Cast<object>().Select(k => new KeyValuePair<object, object>(k, d[k])).ToList();
            }
        }

        private static FieldInfo[] FieldsOf(Type t)
        {
            if (fieldCache.TryGetValue(t, out var cached)) return cached;
            var list = new List<FieldInfo>();
            for (Type x = t; x != null && x != typeof(object); x = x.BaseType)
            {
                foreach (var fi in x.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (fi.FieldType == typeof(Ideo)) continue;
                    if (typeof(Delegate).IsAssignableFrom(fi.FieldType)) continue;
                    list.Add(fi);
                }
            }
            var arr = list.ToArray();
            fieldCache[t] = arr;
            return arr;
        }

        public void RestoreAll(bool includeStrings)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                var fi = fields[i];
                if (!includeStrings && fi.FieldType == typeof(string)) continue;
                try
                {
                    if (!fi.IsInitOnly) fi.SetValue(precept, values[i]);
                    if (listItems[i] != null && values[i] is IList l)
                    {
                        l.Clear();
                        foreach (var o in listItems[i]) l.Add(o);
                    }
                    else if (dictItems[i] != null && values[i] is IDictionary d)
                    {
                        d.Clear();
                        foreach (var kv in dictItems[i]) d[kv.Key] = kv.Value;
                    }
                }
                catch { }
            }
        }

        public void RestoreStrings()
        {
            for (int i = 0; i < fields.Length; i++)
            {
                var fi = fields[i];
                if (fi.FieldType != typeof(string) || fi.IsInitOnly) continue;
                try
                {
                    if (!string.Equals(fi.GetValue(precept) as string, values[i] as string))
                        fi.SetValue(precept, values[i]);
                }
                catch { }
            }
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Name protection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    internal static class NameGuard
    {
        private static readonly HashSet<Precept> protectedSet = new HashSet<Precept>();
        private static List<PreceptState> tailStates;
        private static int tailUntil;

        public static void Protect(IEnumerable<Precept> ps)
        {
            protectedSet.Clear();
            foreach (var p in ps) protectedSet.Add(p);
            tailStates = null;
        }

        public static void Release()
        {
            protectedSet.Clear();
            tailStates = null;
        }

        public static void StartTail(List<PreceptState> states)
        {
            tailStates = states;
            tailUntil = Environment.TickCount + 2000;
        }

        // Re-applies names for ~2 seconds after a restore, in case something renames late
        public static void Tick()
        {
            if (tailStates == null) return;
            if (unchecked(Environment.TickCount - tailUntil) > 0) { Release(); return; }
            if (!KeepIdeoMod.Settings.keepNames) return;
            foreach (var st in tailStates) st.RestoreStrings();
        }

        public static bool Prefix(Precept __instance) =>
            !(KeepIdeoMod.Settings.keepNames && protectedSet.Contains(__instance));

        public static void PatchRegenerateName(Harmony harmony)
        {
            var prefix = new HarmonyMethod(typeof(NameGuard), nameof(Prefix));
            int patched = 0;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (t == null || !typeof(Precept).IsAssignableFrom(t)) continue;
                    MethodInfo m = null;
                    try
                    {
                        m = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                             .FirstOrDefault(x => x.Name == "RegenerateName" && x.GetParameters().Length == 0
                                                  && !x.IsAbstract && x.ReturnType == typeof(void));
                    }
                    catch { }
                    if (m == null) continue;
                    try { harmony.Patch(m, prefix: prefix); patched++; }
                    catch (Exception ex) { Log.Warning("[KeepIdeo] Couldn't patch " + t.FullName + ".RegenerateName: " + ex.Message); }
                }
            }
            if (patched == 0)
                Log.Warning("[KeepIdeo] No Precept.RegenerateName found; names are protected by restore only.");
        }
    }

    // Shift detection without a compile-time dependency on Unity types
    internal static class ShiftKey
    {
        private static bool resolved;
        private static PropertyInfo currentProp, shiftProp;

        public static bool Held()
        {
            if (!resolved)
            {
                resolved = true;
                Type t = AccessTools.TypeByName("UnityEngine.Event");
                currentProp = t?.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
                shiftProp = t?.GetProperty("shift", BindingFlags.Public | BindingFlags.Instance);
            }
            try
            {
                object e = currentProp?.GetValue(null, null);
                return e != null && shiftProp?.GetValue(e, null) is bool b && b;
            }
            catch { return false; }
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Multiplayer guard â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    internal static class MpCompat
    {
        private static bool resolved;
        private static PropertyInfo isInMp;

        public static bool InMultiplayerGame()
        {
            if (Current.ProgramState != ProgramState.Playing) return false;
            if (!resolved)
            {
                resolved = true;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = null;
                    try { t = asm.GetType("Multiplayer.API.MP", false); } catch { }
                    if (t == null) continue;
                    isInMp = t.GetProperty("IsInMultiplayer", BindingFlags.Public | BindingFlags.Static);
                    if (isInMp != null) break;
                }
            }
            try { return isInMp?.GetValue(null, null) is bool b && b; }
            catch { return false; }
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Snapshot / restore â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    internal class Session
    {
        public Ideo ideo;
        public List<MemeDef> memes;
        public readonly Dictionary<Cat, List<Precept>> precepts = new Dictionary<Cat, List<Precept>>();
        public readonly Dictionary<Precept, PreceptState> states = new Dictionary<Precept, PreceptState>();
        public Dictionary<StyleItemDef, object> freqs;
        public List<object> styleCats;
        public List<KeyValuePair<object, object>> styleForThing;
        public bool skip, bypass, seenOpen;
        public int idle;
    }

    internal static class Core
    {
        private static Session current;

        public static void Begin(Ideo ideo, bool initialSelection)
        {
            if (ideo == null) return;

            if (current != null && current.ideo == ideo)
            {
                current.skip |= initialSelection;
                if (current.skip) NameGuard.Release();
                return;
            }

            var s = new Session { ideo = ideo, memes = new List<MemeDef>(ideo.memes ?? new List<MemeDef>()) };
            foreach (Cat c in Enum.GetValues(typeof(Cat)))
                if (c != Cat.None) s.precepts[c] = new List<Precept>();

            foreach (var p in ideo.PreceptsListForReading)
            {
                var c = Classifier.Classify(p);
                if (c == Cat.None) continue;
                s.precepts[c].Add(p);
                try { s.states[p] = new PreceptState(p); }
                catch (Exception e) { Log.Warning("[KeepIdeo] Couldn't snapshot " + Refl.Describe(p) + ": " + e.Message); }
            }

            s.skip = initialSelection || s.memes.Count == 0 || ideo.PreceptsListForReading.Count == 0;

            object style = Refl.IdeoStyle?.GetValue(ideo);
            if (style != null && Refl.GetFrequency != null)
            {
                s.freqs = new Dictionary<StyleItemDef, object>();
                IEnumerable<StyleItemDef> items = DefDatabase<HairDef>.AllDefsListForReading.Cast<StyleItemDef>()
                    .Concat(DefDatabase<BeardDef>.AllDefsListForReading)
                    .Concat(DefDatabase<TattooDef>.AllDefsListForReading);
                foreach (var d in items)
                {
                    try { s.freqs[d] = Refl.GetFrequency.Invoke(style, new object[] { d }); }
                    catch { }
                }
            }

            if (Refl.ThingStyleCategories?.GetValue(ideo) is IList cats)
                s.styleCats = cats.Cast<object>().ToList();

            if (style != null && Refl.StyleForThingDef?.GetValue(style) is IDictionary dict)
                s.styleForThing = dict.Keys.Cast<object>()
                    .Select(k => new KeyValuePair<object, object>(k, dict[k])).ToList();

            current = s;
            if (s.skip) NameGuard.Release();
            else NameGuard.Protect(s.states.Keys);
        }

        public static void MarkBypass()
        {
            if (current != null) current.bypass = true;
            NameGuard.Release();
        }

        public static void OnGUI(WindowStack ws)
        {
            NameGuard.Tick();

            var s = current;
            if (s == null) return;

            if (ws.IsOpen<Dialog_ChooseMemes>() || ws.IsOpen<Dialog_MessageBox>())
            {
                s.seenOpen = true;
                return;
            }

            if (!s.seenOpen)
            {
                if (++s.idle >= 60) { current = null; NameGuard.Release(); }
                return;
            }

            current = null;
            if (s.skip || s.bypass) { NameGuard.Release(); return; }

            if (KeepIdeoMod.Settings.skipInMultiplayer && MpCompat.InMultiplayerGame())
            {
                Log.Message("[KeepIdeo] Multiplayer game detected; leaving vanilla meme reroll alone.");
                NameGuard.Release();
                return;
            }

            try { Restore(s); }
            catch (Exception e) { NameGuard.Release(); Log.Error("[KeepIdeo] Restore failed: " + e); }
        }

        private static bool ValidFor(PreceptDef def, List<MemeDef> memes) =>
            def.requiredMemes.NullOrEmpty() || def.requiredMemes.Any(memes.Contains);

        private static bool RequiredByAny(PreceptDef def, List<MemeDef> added) =>
            !def.requiredMemes.NullOrEmpty() && def.requiredMemes.Any(added.Contains);

        private static void Restore(Session s)
        {
            var set = KeepIdeoMod.Settings;
            Ideo ideo = s.ideo;
            List<MemeDef> newMemes = ideo.memes;
            List<MemeDef> added = newMemes.Where(m => !s.memes.Contains(m)).ToList();
            var keptAll = new List<Precept>();
            int removed = 0, readded = 0, dropped = 0;

            if (set.styles)
            {
                if (s.styleCats != null && Refl.ThingStyleCategories?.GetValue(ideo) is IList cats)
                {
                    cats.Clear();
                    foreach (var o in s.styleCats) cats.Add(o);
                    Refl.SortStyleCategories?.Invoke(ideo, null);
                }
                object st = Refl.IdeoStyle?.GetValue(ideo);
                if (s.styleForThing != null && st != null && Refl.StyleForThingDef?.GetValue(st) is IDictionary dict)
                {
                    dict.Clear();
                    foreach (var kv in s.styleForThing) dict[kv.Key] = kv.Value;
                }
            }

            if (set.appearance && s.freqs != null && Refl.SetFrequency != null)
            {
                object style = Refl.IdeoStyle?.GetValue(ideo);
                if (style != null)
                    foreach (var kv in s.freqs)
                    {
                        try { Refl.SetFrequency.Invoke(style, new[] { (object)kv.Key, kv.Value }); }
                        catch { }
                    }
            }

            foreach (var kv in s.precepts)
            {
                Cat cat = kv.Key;
                if (!set.Enabled(cat)) continue;
                List<Precept> old = kv.Value;

                var keep = new List<Precept>();
                foreach (var p in old)
                {
                    if (set.dropInvalidForNewMemes && !ValidFor(p.def, newMemes))
                    {
                        dropped++;
                        Refl.V(cat + ": dropped " + Refl.Describe(p) + " (needs a meme you removed)");
                    }
                    else keep.Add(p);
                }

                // Modded special precepts: gentle restore (swap back same-def ones, never fight a new meme's stance)
                if (cat == Cat.Other)
                {
                    foreach (var p in keep)
                    {
                        if (ideo.PreceptsListForReading.Contains(p)) { keptAll.Add(p); continue; }
                        var sameDef = ideo.PreceptsListForReading.FirstOrDefault(q => q.def == p.def && !old.Contains(q));
                        if (sameDef != null)
                        {
                            ideo.RemovePrecept(sameDef);
                            removed++;
                        }
                        else if (p.def.issue != null &&
                                 ideo.PreceptsListForReading.Any(q => q.def != null && q.def.issue == p.def.issue))
                        {
                            Refl.V("Other: not restoring " + Refl.Describe(p) + " (a new meme set a different stance on that issue)");
                            continue;
                        }
                        ideo.AddPrecept(p);
                        readded++;
                        keptAll.Add(p);
                        Refl.V("Other: restored " + Refl.Describe(p));
                    }
                    continue;
                }

                List<Precept> now = ideo.PreceptsListForReading.Where(p => Classifier.Classify(p) == cat).ToList();
                var keepDefs = new HashSet<PreceptDef>(keep.Select(p => p.def));

                var extras = new List<Precept>();
                if (set.addRequiredFromNewMemes)
                {
                    foreach (var p in now)
                    {
                        if (keep.Contains(p) || keepDefs.Contains(p.def)) continue;
                        if (!RequiredByAny(p.def, added)) continue;
                        if (extras.Any(e => e.def == p.def)) continue;
                        extras.Add(p);
                    }
                }

                foreach (var p in now)
                {
                    if (keep.Contains(p) || extras.Contains(p)) continue;
                    ideo.RemovePrecept(p);
                    removed++;
                    Refl.V(cat + ": removed rerolled " + Refl.Describe(p));
                }

                foreach (var p in keep)
                {
                    if (!ideo.PreceptsListForReading.Contains(p))
                    {
                        ideo.AddPrecept(p);
                        readded++;
                    }
                    keptAll.Add(p);
                    Refl.V(cat + ": kept " + Refl.Describe(p));
                }

                foreach (var e in extras)
                    Refl.V(cat + ": added (required by new meme) " + Refl.Describe(e));
            }

            // Put back every field of the kept precepts (names, descriptions, settings, assignments)
            var tail = new List<PreceptState>();
            foreach (var p in keptAll)
            {
                if (!s.states.TryGetValue(p, out var st)) continue;
                st.RestoreAll(set.keepNames);
                tail.Add(st);
            }

            Refl.RecachePrecepts?.Invoke(ideo, null);
            NameGuard.StartTail(tail);

            Refl.V("Restored " + ideo.name + ": kept " + keptAll.Count + ", re-added " + readded +
                   ", removed " + removed + " rerolled, dropped " + dropped + ".");
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Harmony patches â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [HarmonyPatch]
    internal static class Patch_ChooseMemes_Ctor
    {
        static IEnumerable<MethodBase> TargetMethods() =>
            typeof(Dialog_ChooseMemes)
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        static void Postfix(Window __instance, object[] __args)
        {
            try
            {
                Ideo ideo = Refl.DialogIdeo?.GetValue(__instance) as Ideo
                            ?? __args?.OfType<Ideo>().FirstOrDefault();
                bool initial = Refl.DialogInitial?.GetValue(__instance) is bool b && b;
                Core.Begin(ideo, initial);
            }
            catch (Exception e)
            {
                Log.Error("[KeepIdeo] Snapshot failed: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(Window), nameof(Window.Close))]
    internal static class Patch_Window_Close
    {
        static void Prefix(Window __instance)
        {
            if (__instance is Dialog_ChooseMemes && ShiftKey.Held())
                Core.MarkBypass();
        }
    }

    [HarmonyPatch(typeof(WindowStack), nameof(WindowStack.WindowStackOnGUI))]
    internal static class Patch_WindowStack_OnGUI
    {
        static void Postfix(WindowStack __instance) => Core.OnGUI(__instance);
    }
}