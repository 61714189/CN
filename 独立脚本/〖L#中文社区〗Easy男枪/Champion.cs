﻿using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

abstract class Champion
{
    protected Obj_AI_Hero Player;
    protected Menu Menu;
    protected Orbwalking.Orbwalker Orbwalker;
    protected SpellManager Spells;

    private int tick = 1000 / 20;
    private int lastTick = Environment.TickCount;
    private string ChampName;
    private SkinManager SkinManager;
    private BushManager BushManager;

    public Champion(string name)
    {
        ChampName = name;

        CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }

    private void Game_OnGameLoad(EventArgs args)
    {
        Player = ObjectManager.Player;

        if (ChampName.ToLower() != Player.ChampionName.ToLower())
            return;

        SkinManager = new SkinManager();
        Spells = new SpellManager();
        BushManager = new BushManager();

        InitializeSpells(ref Spells);
        InitializeSkins(ref SkinManager);

        Menu = new Menu("〖L#中文社区〗Easy男枪", "EasyGraves", true);

        SkinManager.AddToMenu(ref Menu);
        BushManager.AddToMenu(ref Menu);

        Menu.AddSubMenu(new Menu("目标选择", "Target Selector"));
        SimpleTs.AddToMenu(Menu.SubMenu("Target Selector"));

        Menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
        Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

        InitializeMenu();

        Menu.AddItem(new MenuItem("Recall_block", "技能阻止回程").SetValue(true));
        Menu.AddToMainMenu();

        Game.OnGameUpdate += Game_OnGameUpdate;
        Drawing.OnDraw += Drawing_OnDraw;


        Game.PrintChat("<font color = \"#FF0020\">鑴氭湰</font><font color = \"#22FF10\">鍔犺級鎴愬姛锛佹洿澶歸ww.loll35.com  </font>");
        
    }

    private void Drawing_OnDraw(EventArgs args)
    {
        Draw();
    }

    private void Game_OnGameUpdate(EventArgs args)
    {
        if (Environment.TickCount < lastTick + tick) return;
        lastTick = Environment.TickCount;

        SkinManager.Update();

        Update();

        if ((Menu.Item("Recall_block").GetValue<bool>() && Player.HasBuff("Recall")) || Player.IsWindingUp)
            return;

        BushManager.Update(Orbwalker);

        bool minionBlock = false;

        foreach (Obj_AI_Minion minion in MinionManager.GetMinions(Player.Position, Player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
        {
            if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(Player, minion, false))
                minionBlock = true;
        }

        switch (Orbwalker.ActiveMode)
        {
            case Orbwalking.OrbwalkingMode.Combo:
                Combo();
                break;
            case Orbwalking.OrbwalkingMode.Mixed:
                if (!minionBlock) Harass();
                break;
            default:
                if (!minionBlock) Auto();
                break;
        }
    }

    protected virtual void InitializeSkins(ref SkinManager Skins) { }
    protected virtual void InitializeSpells(ref SpellManager Spells) { }
    protected virtual void InitializeMenu() { }

    protected virtual void Update() { }
    protected virtual void Draw() { }
    protected virtual void Combo() { }
    protected virtual void Harass() { }
    protected virtual void Auto() { }

    protected void DrawCircle(string menuItem, string spell)
    {
        Circle circle = Menu.Item(menuItem).GetValue<Circle>();
        if (circle.Active) Utility.DrawCircle(Player.Position, Spells.get(spell).Range, circle.Color);
    }
}