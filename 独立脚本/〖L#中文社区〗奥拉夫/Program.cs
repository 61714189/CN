#region
/*
* Credits to:
 * Eskor
 * Roach_
 * xSalice
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Olafisback
{
    internal class Program
    {
        public const string ChampionName = "Olaf";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell Q2;
        public static Spell W;
        public static Spell E;
        //Items
        
        public static Items.Item HDR;
        public static Items.Item BKR;
        public static Items.Item TMT;
        public static Items.Item BWC;
        public static Items.Item YOU;
        public static Items.Item RAO;

        //Menu
        public static Menu Config;
        private static GameObject _axeObj;
        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 1000);
            Q2 =new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);

            Q.SetSkillshot(0.25f, 75f, 1600f, false, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.25f, 75f, 1600f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(Q2);
            SpellList.Add(W);
            SpellList.Add(E);
            
            //items
            HDR = new Items.Item(3074, 225f);
            TMT = new Items.Item(3077, 225f);
            BKR = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            YOU = new Items.Item(3142, 225f);
            RAO = new Items.Item(3143, 490f);
            

            //Create the menu
            Config = new Menu("〖L#中文社区〗|奥拉夫归来|", "Olaf", true);

            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "使用 物品")).SetValue(true);
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQ2Harass", "使用 短距离|Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Minman", "骚扰最低蓝量|").SetValue(new Slider(30, 100, 0)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("SQRange", "短距离|Q 范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("WRange", "W 范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Game.PrintChat("<font color = \"#FF0020\">鑴氭湰</font><font color = \"#22FF10\">鍔犺級鎴愬姛锛佹洿澶歸ww.loll35.com  </font>");

        }

        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.Name == "olaf_axe_totem_team_id_green.troy")
                _axeObj = obj;
        }

        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.Name == "olaf_axe_totem_team_id_green.troy")
                _axeObj = null;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            {
                if (_axeObj != null)
                    Utility.DrawCircle(_axeObj.Position, 100, Color.Yellow, 6);
            }
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            }
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            if (target.IsValidTarget() && Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady() && Player.Distance(target) <= Q.Range)
            {
                PredictionOutput Qpredict = Q.GetPrediction(target);
                Q.Cast(Qpredict.CastPosition);
            }
            
            if (target.IsValidTarget() && Config.Item("UseECombo").GetValue<bool>() && E.IsReady() && Player.Distance(target) <= E.Range)
            
                E.CastOnUnit(target);
            
            if (target.IsValidTarget() && Config.Item("UseWCombo").GetValue<bool>() && W.IsReady() && Player.Distance(target) <= 225f)
            
                W.Cast();
            
            if (Config.Item("UseItems").GetValue<bool>()) 
            {
                    BKR.Cast(target);
                    
                    BWC.Cast(target);
                    if (Player.Distance(target) <= HDR.Range)
                    {
                        HDR.Cast();
                    }
                    if (Player.Distance(target) <= TMT.Range)
                    {
                        TMT.Cast();
                    }
                    if (Player.Distance(target) <= 400)
                    {
                        YOU.Cast();
                    }
                    if (Player.Distance(target) <= RAO.Range)
                    {
                        RAO.Cast();
                    }
            }
            
            
        }
        private static void Harass()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (target.IsValidTarget() && Q.IsReady() && Config.Item("UseQHarass").GetValue<bool>() && 
                    Player.Mana / Player.MaxMana * 100 > Config.Item("Minman").GetValue<Slider>().Value && Player.Distance(target) <= Q.Range)
            {
                PredictionOutput Qpredict = Q.GetPrediction(target);
                if (Qpredict.Hitchance >= HitChance.High)
                    Q.Cast(Qpredict.CastPosition);
            }
            if (target.IsValidTarget() && Q.IsReady() && Config.Item("UseQ2Harass").GetValue<bool>() && 
                    Player.Mana / Player.MaxMana * 100 > Config.Item("Minman").GetValue<Slider>().Value && Player.Distance(target) <= Q2.Range)
            {
                PredictionOutput Q2predict = Q2.GetPrediction(target);
                if (Q2predict.Hitchance >= HitChance.High)
                    Q2.Cast(Q2predict.CastPosition);
            }
            if (E.IsReady() && Config.Item("UseEHarass").GetValue<bool>() && Player.Distance(target) <= E.Range)
                E.CastOnUnit(target);
        }
       
    }
}
