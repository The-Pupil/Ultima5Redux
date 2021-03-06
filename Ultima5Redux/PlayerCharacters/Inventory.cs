﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using static Ultima5Redux.GameState;
using System.Diagnostics;

namespace Ultima5Redux
{
    public class Inventory
    {
        private List<byte> gameStateByteArray;

        public LordBritishArtifacts Artifacts { get; }
        public ShadowlordShards Shards { get; }
        public Potions MagicPotions { get; }
        public Scrolls MagicScrolls { get; }
        public Spells MagicSpells { get; }
        public SpecialItems SpecializedItems { get; }
        public Armours ProtectiveArmour { get; }
        public Weapons TheWeapons { get; }
        public Reagents SpellReagents { get; }
        public List<InventoryItem> AllItems { get; } = new List<InventoryItem>();
        public List<InventoryItem> ReadyItems { get; } = new List<InventoryItem>();
        public List<InventoryItem> UseItems { get; } = new List<InventoryItem>();
        public enum InventoryThings { Grapple = 0x209, MagicCarpets = 0x20A };

        private byte BoolToByte(bool bBool)
        {
            return bBool ? (byte)1 : (byte)0;
        }

        private void SetInventoryQuantity(InventoryThings thing, byte nThings)
        {
            gameStateByteArray[(int)thing] = nThings;
        }

        private byte GetInventoryQuantity(InventoryThings thing)
        {
            return gameStateByteArray[(int)thing];
        }

        private void SetInventoryBool(InventoryThings thing, bool bBool)
        {
            gameStateByteArray[(int)thing] = BoolToByte(bBool);
        }

        public bool GetInvetoryBool(InventoryThings thing)
        {
            return DataChunk.CreateDataChunk(DataChunk.DataFormatType.Byte, "", gameStateByteArray, (int)thing, sizeof(byte)).GetChunkAsByte() > 0;
        }

        public bool Grapple 
        { 
            get
            {
                return GetInvetoryBool(InventoryThings.Grapple);
            }
            set
            {
                SetInventoryBool(InventoryThings.Grapple, value);
            }
        }

        public int MagicCarpets
        { 
            get
            {
                return GetInventoryQuantity(InventoryThings.MagicCarpets);
            }
            set
            {
                SetInventoryQuantity(InventoryThings.MagicCarpets, (byte)value);
            }
        }

        /// <summary>
        /// Gets the attack of a particular piece of equipment
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        private int GetAttack(DataOvlReference.EQUIPMENT equipment)
        {
            if (TheWeapons.GetWeaponFromEquipment(equipment) != null)
            {
                return TheWeapons.GetWeaponFromEquipment(equipment).AttackStat;
            }
            if (ProtectiveArmour.GetArmourFromEquipment(equipment) != null)
            {
                return (ProtectiveArmour.GetArmourFromEquipment(equipment).AttackStat);
            }
            return 0;
        }

        /// <summary>
        /// Gets the defense of a particular piece of equipment
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        private int GetDefense(DataOvlReference.EQUIPMENT equipment)
        {
            if (TheWeapons.GetWeaponFromEquipment(equipment) != null)
            {
                return TheWeapons.GetWeaponFromEquipment(equipment).AttackStat;
            }
            if (ProtectiveArmour.GetArmourFromEquipment(equipment) != null)
            {
                return (ProtectiveArmour.GetArmourFromEquipment(equipment).DefendStat);
            }
            return 0;
        }

        /// <summary>
        /// Gets the characters total attack if left and right hand both attacked succesfully
        /// </summary>
        /// <param name="record">Character record</param>
        /// <returns>amount of total damage</returns>
        public int GetCharacterTotalAttack (PlayerCharacterRecord record)
        {
            return GetAttack(record.Equipped.Amulet) + GetAttack(record.Equipped.Armor) + GetAttack(record.Equipped.Helmet)
                + GetAttack(record.Equipped.Ring) + GetAttack(record.Equipped.LeftHand) + GetAttack(record.Equipped.RightHand);
        }

        /// <summary>
        /// Gets the players total defense of all items equipped
        /// </summary>
        /// <param name="record">character record</param>
        /// <returns>the players total defense</returns>
        public int GetCharacterTotalDefense (PlayerCharacterRecord record)
        {
            return GetDefense(record.Equipped.Amulet) + GetDefense(record.Equipped.Armor) + GetDefense(record.Equipped.Helmet) +
                GetDefense(record.Equipped.LeftHand) + GetDefense(record.Equipped.RightHand) + GetDefense(record.Equipped.Ring);
        }


        /// <summary>
        /// Gets the Combat Item (inventory item) based on the equipped item
        /// </summary>
        /// <param name="equipment">type of combat equipment</param>
        /// <returns>combat item object</returns>
        public CombatItem GetItemFromEquipment(DataOvlReference.EQUIPMENT equipment)
        {
            foreach (CombatItem item in ReadyItems)
            {
                if (item.SpecificEquipment == equipment)
                {
                    return item;
                }
            }
            throw new Exception("Requested " + equipment.ToString() + " but is not a combat type");
        }

        public Inventory(List<byte> gameStateByteArray, DataOvlReference dataOvlRef)
        {
            this.gameStateByteArray = gameStateByteArray;

            DataChunk.CreateDataChunk(DataChunk.DataFormatType.Byte, "Grapple", gameStateByteArray, 0x209, sizeof(byte));
            DataChunk.CreateDataChunk(DataChunk.DataFormatType.Byte, "Magic Carpet", gameStateByteArray, 0x20A, sizeof(byte));

            ProtectiveArmour = new Armours(dataOvlRef, gameStateByteArray);
            AllItems.AddRange(ProtectiveArmour.GenericItemList);
            ReadyItems.AddRange(ProtectiveArmour.GenericItemList);

            TheWeapons = new Weapons(dataOvlRef, gameStateByteArray);
            ReadyItems.AddRange(TheWeapons.GenericItemList);

            MagicScrolls = new Scrolls(dataOvlRef, gameStateByteArray);
            UseItems.AddRange(MagicScrolls.GenericItemList);

            MagicPotions = new Potions(dataOvlRef, gameStateByteArray);
            UseItems.AddRange(MagicPotions.GenericItemList);

            SpecializedItems = new SpecialItems(dataOvlRef, gameStateByteArray);
            AllItems.AddRange(SpecializedItems.GenericItemList);
            UseItems.AddRange(SpecializedItems.GenericItemList);

            Artifacts = new LordBritishArtifacts(dataOvlRef, gameStateByteArray);
            AllItems.AddRange(Artifacts.GenericItemList);
            UseItems.AddRange(Artifacts.GenericItemList);

            Shards = new ShadowlordShards(dataOvlRef, gameStateByteArray);
            AllItems.AddRange(Shards.GenericItemList);
            UseItems.AddRange(Shards.GenericItemList);

            SpellReagents = new Reagents(dataOvlRef, gameStateByteArray);
            AllItems.AddRange(SpellReagents.GenericItemList);

            MagicSpells = new Spells(dataOvlRef, gameStateByteArray);
            AllItems.AddRange(MagicSpells.GenericItemList);


        }

    }
}
