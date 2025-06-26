using System.Numerics;
using Terraria;
using Terraria.ID;

namespace MyPlugin;

internal class MyUtils
{
    #region 实现社交栏饰品功能方法（自己改了点变量取值,但参数没改）
    public static void ApplyEquipFunctional(Player plr, Item item)
    {
        if (item.type == 3809 || item.type == 3810 || item.type == 3811 || item.type == 3812)
        {
            plr.dd2Accessory = true;
        }

        switch (item.type)
        {
            case 4056:
                plr.chiselSpeed = true;
                break;
            case 3990:
                plr.accRunSpeed = 6f;
                plr.autoJump = true;
                plr.jumpSpeedBoost += 1.6f;
                plr.extraFall += 10;
                break;
            case 3991:
                plr.manaFlower = true;
                plr.manaCost -= 0.08f;
                plr.aggro -= 400;
                break;
            case 3992:
                plr.kbGlove = true;
                plr.autoReuseGlove = true;
                plr.meleeScaleGlove = true;
                plr.meleeSpeed += 0.12f;
                plr.aggro += 400;
                break;
            case 3993:
                plr.accRunSpeed = 6f;
                plr.rocketBoots = plr.vanityRocketBoots = 2;
                break;
            case 4055:
                plr.accRunSpeed = 6f;
                plr.desertBoots = true;
                break;
            case 3994:
                plr.autoJump = true;
                plr.jumpSpeedBoost += 1.6f;
                plr.extraFall += 10;
                plr.accFlipper = true;
                break;
            case 3995:
                plr.autoJump = true;
                plr.jumpSpeedBoost += 1.6f;
                plr.extraFall += 10;
                plr.accFlipper = true;
                plr.spikedBoots += 2;
                break;
            case 3996:
                plr.autoJump = true;
                plr.jumpSpeedBoost += 1.6f;
                plr.extraFall += 10;
                plr.spikedBoots += 2;
                break;
            case 3998:
                plr.aggro += 400;
                break;
            case 4038:
                plr.fireWalk = true;
                break;
            case 4003:
                plr.fireWalk = true;
                plr.lavaRose = true;
                break;
            case 4000:
                plr.manaFlower = true;
                plr.manaCost -= 0.08f;
                plr.manaMagnet = true;
                break;
            case 4001:
                plr.manaFlower = true;
                plr.manaCost -= 0.08f;
                plr.starCloakItem = item;
                plr.starCloakItem_manaCloakOverrideItem = item;
                break;
            case 4002:
                plr.magicQuiver = true;
                plr.arrowDamageAdditiveStack += 0.1f;
                plr.hasMoltenQuiver = true;
                break;
            case 4004:
                plr.fireWalk = true;
                plr.lavaRose = true;
                break;
            case 3999:
                plr.fireWalk = true;
                break;
            case 4005:
                plr.rangedCrit += (int)10f;
                plr.rangedDamage += 0.1f;
                plr.aggro -= 400;
                break;
            case 4006:
                plr.aggro -= 400;
                plr.magicQuiver = true;
                plr.arrowDamageAdditiveStack += 0.1f;
                break;
            case 4007:
                plr.honeyCombItem = item;
                plr.armorPenetration += 5;
                break;
            case 4341:
            case 5126:
                plr.portableStoolInfo.SetStats(26, 26, 26);
                break;
            case 4409:
                plr.CanSeeInvisibleBlocks = true;
                break;
            case 5010:
                plr.treasureMagnet = true;
                break;
            case 3245:
                plr.boneGloveItem = item;
                break;
            case 5107:
                plr.hasMagiluminescence = true;
                if (Main.netMode != 2) // 不在服务端执行渲染光照
                {
                    plr.MountedCenter.ToTileCoordinates();
                    DelegateMethods.v3_1 = new Vector3(0.9f, 0.8f, 0.5f);
                    Utils.PlotTileLine(plr.Center, plr.Center + plr.velocity * 6f, 20f, DelegateMethods.CastLightOpen);
                    Utils.PlotTileLine(plr.Left, plr.Right, 20f, DelegateMethods.CastLightOpen);
                }
                break;
        }

        if (item.type == 3015)
        {
            plr.aggro -= 400;
            plr.meleeCrit += 5;
            plr.magicCrit += 5;
            plr.rangedCrit += 5;
            plr.meleeDamage += 0.05f;
            plr.magicDamage += 0.05f;
            plr.rangedDamage += 0.05f;
            plr.minionDamage += 0.05f;
        }

        if (item.type == 3016)
        {
            plr.aggro += 400;
        }

        if (item.type == 2373)
        {
            plr.accFishingLine = true;
        }

        if (item.type == 2374)
        {
            plr.fishingSkill += 10;
        }

        if (item.type == 5139 || item.type == 5144 || item.type == 5142 || item.type == 5141 || item.type == 5146 || item.type == 5140 || item.type == 5145 || item.type == 5143)
        {
            plr.accFishingBobber = true;
        }

        if (item.type == 2375)
        {
            plr.accTackleBox = true;
        }
        if (item.type == 4881)
        {
            plr.accLavaFishing = true;
        }

        if (item.type == 3721)
        {
            plr.accFishingLine = true;
            plr.accTackleBox = true;
            plr.fishingSkill += 10;
        }

        if (item.type == 5064)
        {
            plr.accFishingLine = true;
            plr.accTackleBox = true;
            plr.fishingSkill += 10;
            plr.accLavaFishing = true;
        }

        if (item.type == 3090)
        {
            plr.npcTypeNoAggro[1] = true;
            plr.npcTypeNoAggro[16] = true;
            plr.npcTypeNoAggro[59] = true;
            plr.npcTypeNoAggro[71] = true;
            plr.npcTypeNoAggro[81] = true;
            plr.npcTypeNoAggro[138] = true;
            plr.npcTypeNoAggro[121] = true;
            plr.npcTypeNoAggro[122] = true;
            plr.npcTypeNoAggro[141] = true;
            plr.npcTypeNoAggro[147] = true;
            plr.npcTypeNoAggro[183] = true;
            plr.npcTypeNoAggro[184] = true;
            plr.npcTypeNoAggro[204] = true;
            plr.npcTypeNoAggro[225] = true;
            plr.npcTypeNoAggro[244] = true;
            plr.npcTypeNoAggro[302] = true;
            plr.npcTypeNoAggro[333] = true;
            plr.npcTypeNoAggro[335] = true;
            plr.npcTypeNoAggro[334] = true;
            plr.npcTypeNoAggro[336] = true;
            plr.npcTypeNoAggro[537] = true;
            plr.npcTypeNoAggro[676] = true;
            plr.npcTypeNoAggro[667] = true;
        }

        if (item.stringColor > 0)
        {
            plr.yoyoString = true;
        }

        if (item.type == 3366)
        {
            plr.counterWeight = 556 + Main.rand.Next(6);
            plr.yoyoGlove = true;
            plr.yoyoString = true;
        }

        if (item.type >= 3309 && item.type <= 3314)
        {
            plr.counterWeight = 556 + item.type - 3309;
        }

        if (item.type == 3334)
        {
            plr.yoyoGlove = true;
        }

        if (item.type == 3337)
        {
            plr.shinyStone = true;
        }

        if (item.type == 4989)
        {
            plr.empressBrooch = true;
            plr.moveSpeed += 0.075f;
        }

        if (item.type == 3336)
        {
            plr.SporeSac(item);
            plr.sporeSac = true;
        }

        if (item.type == 4987)
        {
            plr.VolatileGelatin(item);
            plr.volatileGelatin = true;
        }

        switch (item.type)
        {
            case 3538:
                plr.stardustMonolithShader = true;
                break;
            case 3537:
                plr.nebulaMonolithShader = true;
                break;
            case 3536:
                plr.vortexMonolithShader = true;
                break;
            case 3539:
                plr.solarMonolithShader = true;
                break;
            case 4318:
                plr.moonLordMonolithShader = true;
                break;
            case 4054:
                plr.bloodMoonMonolithShader = true;
                break;
            case 5345:
                plr.CanSeeInvisibleBlocks = true;
                break;
            case 5347:
                plr.shimmerMonolithShader = true;
                break;
        }

        if (item.type == 5113)
        {
            plr.dontStarveShader = !plr.dontStarveShader;
        }

        if (item.type == 2423)
        {
            plr.autoJump = true;
            plr.jumpSpeedBoost += 1.6f;
            plr.extraFall += 10;
        }

        if (item.type == 857)
        {
            plr.hasJumpOption_Sandstorm = true;
        }

        if (item.type == 983)
        {
            plr.hasJumpOption_Sandstorm = true;
            plr.jumpBoost = true;
        }

        if (item.type == 987)
        {
            plr.hasJumpOption_Blizzard = true;
        }

        if (item.type == 1163)
        {
            plr.hasJumpOption_Blizzard = true;
            plr.jumpBoost = true;
        }

        if (item.type == 1724)
        {
            plr.hasJumpOption_Fart = true;
        }

        if (item.type == 1863)
        {
            plr.hasJumpOption_Fart = true;
            plr.jumpBoost = true;
        }

        if (item.type == 1164)
        {
            // 气球（跳跃加速）
            plr.jumpBoost = true;
            // 云朵瓶
            plr.hasJumpOption_Cloud = true;
            // 沙暴瓶
            plr.hasJumpOption_Sandstorm = true;
            // 暴雪瓶
            plr.hasJumpOption_Blizzard = true;
            // 海啸瓶
            plr.hasJumpOption_Sail = true;
            // 瓶中臭屁
            plr.hasJumpOption_Fart = true;
            // 恩赐苹果
            plr.hasJumpOption_Unicorn = true;
            // 玩具坦克
            plr.hasJumpOption_Santank = true;
            // 山羊骷髅头
            plr.hasJumpOption_WallOfFleshGoat = true;
            // 远古号角
            plr.hasJumpOption_Basilisk = true;
        }

        if (item.type == 5331)
        {
            plr.noFallDmg = true;
            plr.hasLuck_LuckyHorseshoe = true;
            // 气球（跳跃加速）
            plr.jumpBoost = true;
            // 云朵瓶
            plr.hasJumpOption_Cloud = true;
            // 沙暴瓶
            plr.hasJumpOption_Sandstorm = true;
            // 暴雪瓶
            plr.hasJumpOption_Blizzard = true;
            // 海啸瓶
            plr.hasJumpOption_Sail = true;
            // 瓶中臭屁
            plr.hasJumpOption_Fart = true;
            // 恩赐苹果
            plr.hasJumpOption_Unicorn = true;
            // 玩具坦克
            plr.hasJumpOption_Santank = true;
            // 山羊骷髅头
            plr.hasJumpOption_WallOfFleshGoat = true;
            // 远古号角
            plr.hasJumpOption_Basilisk = true;
        }

        if (item.type == 1250)
        {
            plr.jumpBoost = true;
            plr.hasJumpOption_Cloud = true;
            plr.noFallDmg = true;
            plr.hasLuck_LuckyHorseshoe = true;
        }

        if (item.type == 1252)
        {
            plr.hasJumpOption_Sandstorm = true;
            plr.jumpBoost = true;
            plr.noFallDmg = true;
            plr.hasLuck_LuckyHorseshoe = true;
        }

        if (item.type == 1251)
        {
            plr.hasJumpOption_Blizzard = true;
            plr.jumpBoost = true;
            plr.noFallDmg = true;
            plr.hasLuck_LuckyHorseshoe = true;
        }

        if (item.type == 3250)
        {
            plr.hasJumpOption_Fart = true;
            plr.jumpBoost = true;
            plr.noFallDmg = true;
            plr.hasLuck_LuckyHorseshoe = true;
        }

        if (item.type == 3252)
        {
            plr.hasJumpOption_Sail = true;
            plr.jumpBoost = true;
            plr.noFallDmg = true;
            plr.hasLuck_LuckyHorseshoe = true;
        }

        if (item.type == 3251)
        {
            plr.jumpBoost = true;
            plr.honeyCombItem = item;
            plr.noFallDmg = true;
            plr.hasLuck_LuckyHorseshoe = true;
        }

        if (item.type == 1249)
        {
            plr.jumpBoost = true;
            plr.honeyCombItem = item;
        }

        if (item.type == 3241)
        {
            plr.jumpBoost = true;
            plr.hasJumpOption_Sail = true;
        }

        if ((item.type == 1253 || item.type == 3997) && (double)plr.statLife <= (double)plr.statLifeMax2 * 0.5)
        {
            plr.AddBuff(62, 120); //原5帧 改2秒
        }

        if (item.type == 1290)
        {
            plr.panic = true;
        }

        if ((item.type == 1300 || item.type == 1858 || item.type == 4005) && (plr.inventory[plr.selectedItem].useAmmo == AmmoID.Bullet || plr.inventory[plr.selectedItem].useAmmo == AmmoID.CandyCorn || plr.inventory[plr.selectedItem].useAmmo == AmmoID.Stake || plr.inventory[plr.selectedItem].useAmmo == 23 || plr.inventory[plr.selectedItem].useAmmo == AmmoID.Solution))
        {
            plr.scope = true;
        }

        if (item.type == 1858)
        {
            plr.rangedCrit += 10;
            plr.rangedDamage += 0.1f;
        }

        if (item.type == 1301)
        {
            plr.meleeCrit += 8;
            plr.rangedCrit += 8;
            plr.magicCrit += 8;
            plr.meleeDamage += 0.1f;
            plr.rangedDamage += 0.1f;
            plr.magicDamage += 0.1f;
            plr.minionDamage += 0.1f;
        }

        if (item.type == 111)
        {
            plr.statManaMax2 += 20;
        }

        if (item.type == 982)
        {
            plr.statManaMax2 += 20;
            plr.manaRegenDelayBonus += 1f;
            plr.manaRegenBonus += 25;
        }

        if (item.type == 1595)
        {
            plr.statManaMax2 += 20;
            plr.magicCuffs = true;
        }

        if (item.type == 2219)
        {
            plr.manaMagnet = true;
        }

        if (item.type == 2220)
        {
            plr.manaMagnet = true;
            plr.magicDamage += 0.15f;
        }

        if (item.type == 2221)
        {
            plr.manaMagnet = true;
            plr.statManaMax2 += 20;
            plr.magicCuffs = true;
        }

        if (plr.whoAmI == Main.myPlayer && item.type == 1923)
        {
            Player.tileRangeX++;
            Player.tileRangeY++;
        }

        if (item.type == 1247)
        {
            plr.starCloakItem = item;
            plr.honeyCombItem = item;
            plr.starCloakItem_beeCloakOverrideItem = item;
        }

        if (item.type == 1248)
        {
            plr.meleeCrit += 10;
            plr.rangedCrit += 10;
            plr.magicCrit += 10;
        }

        if (item.type == 854)
        {
            plr.discountEquipped = true;
        }

        if (item.type == 855)
        {
            plr.hasLuckyCoin = true;
            plr.hasLuck_LuckyCoin = true;
        }

        if (item.type == 3033)
        {
            plr.goldRing = true;
        }

        if (item.type == 3034)
        {
            plr.goldRing = true;
            plr.hasLuckyCoin = true;
            plr.hasLuck_LuckyCoin = true;
        }

        if (item.type == 3035)
        {
            plr.goldRing = true;
            plr.hasLuckyCoin = true;
            plr.hasLuck_LuckyCoin = true;
            plr.discountEquipped = true;
        }

        if (item.type == 53)
        {
            plr.hasJumpOption_Cloud = true;
        }

        if (item.type == 3201)
        {
            plr.hasJumpOption_Sail = true;
        }

        if (item.type == 54)
        {
            plr.accRunSpeed = 6f;
        }

        if (item.type == 3068)
        {
            plr.cordage = true;
        }

        if (item.type == 1579)
        {
            plr.accRunSpeed = 6f;
        }

        if (item.type == 3200)
        {
            plr.accRunSpeed = 6f;
        }

        if (item.type == 128)
        {
            plr.rocketBoots = (plr.vanityRocketBoots = 1);
        }

        if (item.type == 156)
        {
            plr.noKnockback = true;
        }

        if (item.type == 158)
        {
            plr.noFallDmg = true;
            plr.hasLuck_LuckyHorseshoe = true;
        }

        if (item.type == 934)
        {
            plr.carpet = true;
        }

        if (item.type == 953)
        {
            plr.spikedBoots++;
        }

        if (item.type == 975)
        {
            plr.spikedBoots++;
        }

        if (item.type == 976)
        {
            plr.spikedBoots += 2;
        }

        if (item.type == 977)
        {
            plr.dashType = 1;
        }

        if (item.type == 3097)
        {
            plr.dashType = 2;
        }

        if (item.type == 963)
        {
            plr.blackBelt = true;
        }

        if (item.type == 984)
        {
            plr.blackBelt = true;
            plr.dashType = 1;
            plr.spikedBoots = 2;
        }

        if (item.type == 1131)
        {
            plr.gravControl2 = true;
        }

        if (item.type == 1132)
        {
            plr.honeyCombItem = item;
        }

        if (item.type == 1578)
        {
            plr.honeyCombItem = item;
            plr.panic = true;
        }

        if (item.type == 3224)
        {
            plr.endurance += 0.17f;
        }

        if (item.type == 3223)
        {
            plr.brainOfConfusionItem = item;
        }

        if (item.type == 950)
        {
            plr.iceSkate = true;
        }

        if (item.type == 159)
        {
            plr.jumpBoost = true;
        }

        if (item.type == 3225)
        {
            plr.jumpBoost = true;
        }

        if (item.type == 187)
        {
            plr.accFlipper = true;
        }

        if (item.type == 211)
        {
            plr.autoReuseGlove = true;
            plr.meleeSpeed += 0.12f;
        }

        if (item.type == 223)
        {
            plr.manaCost -= 0.06f;
        }

        if (item.type == 285)
        {
            plr.moveSpeed += 0.05f;
        }

        if (item.type == 212)
        {
            plr.moveSpeed += 0.1f;
        }

        if (item.type == 267)
        {
            plr.killGuide = true;
        }

        if (item.type == 1307)
        {
            plr.killClothier = true;
        }

        if (item.type == 193)
        {
            plr.fireWalk = true;
        }

        if (item.type == 861)
        {
            plr.accMerman = true;
            plr.wolfAcc = true;
            if (plr.hideVisibleAccessory[item.whoAmI])
            {
                plr.hideMerman = true;
                plr.hideWolf = true;
            }
        }

        if (item.type == 862)
        {
            plr.starCloakItem = item;
            plr.longInvince = true;
            plr.starCloakItem_starVeilOverrideItem = item;
        }

        if (item.type == 860)
        {
            plr.pStone = true;
        }

        if (item.type == 863)
        {
            plr.waterWalk2 = true;
        }

        if (item.type == 907)
        {
            plr.waterWalk2 = true;
            plr.fireWalk = true;
        }

        if (item.type == 5044)
        {
            plr.hasCreditsSceneMusicBox = true;
        }

        if (item.type == 908 || item.type == 5000)
        {
            plr.waterWalk = true;
            plr.fireWalk = true;
            plr.lavaMax += 420;
            plr.lavaRose = true;
        }

        if ((!plr.mount.Active || plr.mount.Type != 47) && !plr.hideVisibleAccessory[item.whoAmI] && (item.type == 4822 || item.type == 4874))
        {
            plr.DoBootsEffect(plr.DoBootsEffect_PlaceFlamesOnTile);
        }

        if (item.type == 906 || item.type == 4038 || item.type == 3999 || item.type == 4003)
        {
            plr.lavaMax += 420;
        }

        if (item.type == 485)
        {
            plr.wolfAcc = true;
            if (plr.hideVisibleAccessory[item.whoAmI])
            {
                plr.hideWolf = true;
            }
        }

        if (item.type == 486)
        {
            plr.rulerLine = true;
        }

        if (item.type == 2799)
        {
            plr.rulerGrid = true;
        }

        if (item.type == 394)
        {
            plr.accFlipper = true;
            plr.accDivingHelm = true;
        }

        if (item.type == 396)
        {
            plr.noFallDmg = true;
            plr.fireWalk = true;
            plr.hasLuck_LuckyHorseshoe = true;
        }

        if (item.type == 397)
        {
            plr.noKnockback = true;
            plr.fireWalk = true;
        }

        if (item.type == 399)
        {
            plr.jumpBoost = true;
            plr.hasJumpOption_Cloud = true;
        }

        if (item.type == 405)
        {
            plr.accRunSpeed = 6f;
            plr.rocketBoots = (plr.vanityRocketBoots = 2);
        }

        if (item.type == 1303)
        {
            if (!plr.wet)
            {
                Lighting.AddLight((int)plr.Center.X / 16, (int)plr.Center.Y / 16, 0.225f, 0.05f, 0.15f);
            }
            if (plr.wet)
            {
                Lighting.AddLight((int)plr.Center.X / 16, (int)plr.Center.Y / 16, 1.8f, 0.4f, 1.2f);
            }
        }

        if (item.type == 1860)
        {
            plr.accFlipper = true;
            plr.accDivingHelm = true;
            if (!plr.wet)
            {
                Lighting.AddLight((int)plr.Center.X / 16, (int)plr.Center.Y / 16, 0.225f, 0.05f, 0.15f);
            }
            if (plr.wet)
            {
                Lighting.AddLight((int)plr.Center.X / 16, (int)plr.Center.Y / 16, 1.8f, 0.4f, 1.2f);
            }
        }

        if (item.type == 1861)
        {
            plr.arcticDivingGear = true;
            plr.accFlipper = true;
            plr.accDivingHelm = true;
            plr.iceSkate = true;
            if (!plr.wet)
            {
                Lighting.AddLight((int)plr.Center.X / 16, (int)plr.Center.Y / 16, 0.05f, 0.15f, 0.225f);
            }
            if (plr.wet)
            {
                Lighting.AddLight((int)plr.Center.X / 16, (int)plr.Center.Y / 16, 0.4f, 1.2f, 1.8f);
            }
        }

        if (item.type == 2214)
        {
            plr.equippedAnyTileSpeedAcc = true;
        }

        if (item.type == 2215)
        {
            plr.equippedAnyTileRangeAcc = true;
        }

        if (item.type == 2216)
        {
            plr.autoPaint = true;
        }

        if (item.type == 2217)
        {
            plr.equippedAnyWallSpeedAcc = true;
        }

        if (item.type == 3061)
        {
            plr.equippedAnyWallSpeedAcc = true;
            plr.equippedAnyTileSpeedAcc = true;
            plr.autoPaint = true;
            plr.equippedAnyTileRangeAcc = true;
        }

        if (item.type == 5126)
        {
            plr.equippedAnyWallSpeedAcc = true;
            plr.equippedAnyTileSpeedAcc = true;
            plr.autoPaint = true;
            plr.equippedAnyTileRangeAcc = true;
            plr.treasureMagnet = true;
            plr.chiselSpeed = true;
        }

        if (item.type == 3624)
        {
            plr.autoActuator = true;
        }

        if (item.type == 897)
        {
            plr.kbGlove = true;
            plr.autoReuseGlove = true;
            plr.meleeScaleGlove = true;
            plr.meleeSpeed += 0.12f;
        }

        if (item.type == 1343)
        {
            plr.kbGlove = true;
            plr.autoReuseGlove = true;
            plr.meleeScaleGlove = true;
            plr.meleeSpeed += 0.12f;
            plr.meleeDamage += 0.12f;
            plr.magmaStone = true;
        }

        if (item.type == 1167)
        {
            plr.minionKB += 2f;
            plr.minionDamage += 0.15f;
        }

        if (item.type == 1864)
        {
            plr.minionKB += 2f;
            plr.minionDamage += 0.15f;
            plr.maxMinions++;
        }

        if (item.type == 1845)
        {
            plr.minionDamage += 0.1f;
            plr.maxMinions++;
        }

        if (item.type == 1321)
        {
            plr.magicQuiver = true;
            plr.arrowDamageAdditiveStack += 0.1f;
        }
        if (item.type == 1322)
        {
            plr.magmaStone = true;
        }

        if (item.type == 1323)
        {
            plr.lavaRose = true;
        }

        if (item.type == 3333)
        {
            plr.strongBees = true;
        }

        if (item.type == 938 || item.type == 3997 || item.type == 3998)
        {
            plr.noKnockback = true;
            if (plr.statLife > plr.statLifeMax2 * 0.25f)
            {
                plr.hasPaladinShield = true;
                if (plr.whoAmI != Main.myPlayer && plr.miscCounter % 10 == 0)
                {
                    int myPlayer = Main.myPlayer;
                    if (Main.player[myPlayer].team == plr.team && plr.team != 0)
                    {
                        float num4 = plr.position.X - Main.player[myPlayer].position.X;
                        float num2 = plr.position.Y - Main.player[myPlayer].position.Y;
                        if ((float)Math.Sqrt(num4 * num4 + num2 * num2) < 800f)
                        {
                            Main.player[myPlayer].AddBuff(43, 120); // 原20帧 改2秒
                        }
                    }
                }
            }
        }

        if (item.type == 936)
        {
            plr.kbGlove = true;
            plr.autoReuseGlove = true;
            plr.meleeScaleGlove = true;
            plr.meleeSpeed += 0.12f;
            plr.meleeDamage += 0.12f;
        }

        if (item.type == 898)
        {
            plr.accRunSpeed = 6.75f;
            plr.rocketBoots = (plr.vanityRocketBoots = 2);
            plr.moveSpeed += 0.08f;
        }

        if (item.type == 1862)
        {
            plr.accRunSpeed = 6.75f;
            plr.rocketBoots = (plr.vanityRocketBoots = 3);
            plr.moveSpeed += 0.08f;
            plr.iceSkate = true;
        }

        if (item.type == 5000)
        {
            plr.accRunSpeed = 6.75f;
            plr.rocketBoots = (plr.vanityRocketBoots = 4);
            plr.moveSpeed += 0.08f;
            plr.iceSkate = true;
        }

        if (item.type == 4874)
        {
            plr.accRunSpeed = 6f;
            plr.rocketBoots = (plr.vanityRocketBoots = 5);
        }

        if (item.type == 3110)
        {
            plr.accMerman = true;
            plr.wolfAcc = true;
            if (plr.hideVisibleAccessory[item.whoAmI])
            {
                plr.hideMerman = true;
                plr.hideWolf = true;
            }
        }

        if (item.type == 1865 || item.type == 3110)
        {
            plr.skyStoneEffects = true;
        }

        if (item.type == 899 && Main.dayTime)
        {
            plr.skyStoneEffects = true;
        }

        if (item.type == 900 && (!Main.dayTime || Main.eclipse))
        {
            plr.skyStoneEffects = true;
        }

        if (item.type == 407)
        {
            plr.blockRange++;
        }

        if (item.type == 489)
        {
            plr.magicDamage += 0.15f;
        }

        if (item.type == 490)
        {
            plr.meleeDamage += 0.15f;
        }

        if (item.type == 491)
        {
            plr.rangedDamage += 0.15f;
        }

        if (item.type == 2998)
        {
            plr.minionDamage += 0.15f;
        }

        if (item.type == 935)
        {
            plr.magicDamage += 0.12f;
            plr.meleeDamage += 0.12f;
            plr.rangedDamage += 0.12f;
            plr.minionDamage += 0.12f;
        }

        if (item.wingSlot != -1)
        {
            plr.wingTimeMax = plr.GetWingStats(item.wingSlot).FlyTime;
        }

        if (item.wingSlot == 26)
        {
            plr.ignoreWater = true;
        }

        if (item.type == 5452)
        {
            plr.remoteVisionForDrone = true;
        }

        if (item.type == 885)
        {
            plr.buffImmune[30] = true;
        }

        if (item.type == 886)
        {
            plr.buffImmune[36] = true;
        }

        if (item.type == 887)
        {
            plr.buffImmune[20] = true;
        }

        if (item.type == 888)
        {
            plr.buffImmune[22] = true;
        }

        if (item.type == 889)
        {
            plr.buffImmune[32] = true;
        }

        if (item.type == 890)
        {
            plr.buffImmune[35] = true;
        }

        if (item.type == 891)
        {
            plr.buffImmune[23] = true;
        }

        if (item.type == 892)
        {
            plr.buffImmune[33] = true;
        }

        if (item.type == 893)
        {
            plr.buffImmune[31] = true;
        }

        if (item.type == 3781)
        {
            plr.buffImmune[156] = true;
        }

        if (item.type == 901)
        {
            plr.buffImmune[33] = true;
            plr.buffImmune[36] = true;
        }

        if (item.type == 902)
        {
            plr.buffImmune[30] = true;
            plr.buffImmune[20] = true;
        }

        if (item.type == 903)
        {
            plr.buffImmune[32] = true;
            plr.buffImmune[31] = true;
        }

        if (item.type == 904)
        {
            plr.buffImmune[35] = true;
            plr.buffImmune[23] = true;
        }

        if (item.type == 5354)
        {
            plr.buffImmune[22] = true;
            plr.buffImmune[156] = true;
        }

        if (item.type == 1921)
        {
            plr.buffImmune[46] = true;
            plr.buffImmune[47] = true;
        }

        if (item.type == 1612)
        {
            plr.buffImmune[33] = true;
            plr.buffImmune[36] = true;
            plr.buffImmune[30] = true;
            plr.buffImmune[20] = true;
            plr.buffImmune[32] = true;
            plr.buffImmune[31] = true;
            plr.buffImmune[35] = true;
            plr.buffImmune[23] = true;
            plr.buffImmune[22] = true;
            plr.buffImmune[156] = true;
        }

        if (item.type == 1613)
        {
            plr.buffImmune[46] = true;
            plr.noKnockback = true;
            plr.fireWalk = true;
            plr.buffImmune[33] = true;
            plr.buffImmune[36] = true;
            plr.buffImmune[30] = true;
            plr.buffImmune[20] = true;
            plr.buffImmune[32] = true;
            plr.buffImmune[31] = true;
            plr.buffImmune[35] = true;
            plr.buffImmune[23] = true;
            plr.buffImmune[22] = true;
            plr.buffImmune[156] = true;
        }

        if (item.type == 497)
        {
            plr.accMerman = true;
            if (plr.hideVisibleAccessory[item.whoAmI])
            {
                plr.hideMerman = true;
            }
        }

        if (item.type == 535)
        {
            plr.pStone = true;
        }

        if (item.type == 536)
        {
            plr.kbGlove = true;
            plr.meleeScaleGlove = true;
        }

        if (item.type == 532)
        {
            plr.starCloakItem = item;
        }

        if (item.type == 554)
        {
            plr.longInvince = true;
        }

        if (item.type == 555)
        {
            plr.manaFlower = true;
            plr.manaCost -= 0.08f;
        }

        if (item.wingSlot > 0)
        {
            if (!plr.hideVisibleAccessory[item.whoAmI] || (plr.velocity.Y != 0f && !plr.mount.Active))
            {
                plr.wings = item.wingSlot;
            }

            plr.wingsLogic = item.wingSlot;
        }

        //省略音乐盒、威尔逊胡子的饰品功能处理

        // 发送网络同步消息
        if (Main.netMode == 2)
        {
            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, plr.whoAmI);
            NetMessage.SendData(MessageID.SyncPlayerZone, -1, -1, null, plr.whoAmI);
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, plr.whoAmI);
        }
    }
    #endregion
}
