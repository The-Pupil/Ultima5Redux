﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Ultima5Redux
{
    public class GameState
    {
        #region Private Fields
        /// <summary>
        /// A random number generator - capable of seeding in future
        /// </summary>
        private Random ran = new Random();

        /// <summary>
        /// Game State raw data
        /// </summary>
        private DataChunks<DataChunkName> dataChunks;

        private DataChunks<OverlayChunkName> overworldOverlayDataChunks;

        private DataChunks<OverlayChunkName> underworldOverlayDataChunks;

        /// <summary>
        /// 2D array of flag indicating if an NPC is met [mastermap][npc#]
        /// </summary>
        private bool[][] npcIsMetArray;

        /// <summary>
        /// 2D array of flag indicating if an NPC is dead [mastermap][npc#]
        /// </summary>
        private bool[][] npcIsDeadArray;

        private DataOvlReference dataRef;

        /// <summary>
        /// All player character records
        /// </summary>
        public PlayerCharacterRecords CharacterRecords { get; }
        #endregion

        #region Internal Properties for direct save memory access
        internal DataChunk CharacterAnimationStatesDataChunk { get { return dataChunks.GetDataChunk(DataChunkName.CHARACTER_ANIMATION_STATES); } }
        internal DataChunk CharacterStatesDataChunk { get { return dataChunks.GetDataChunk(DataChunkName.CHARACTER_STATES); } }
        internal DataChunk NonPlayerCharacterMovementLists { get { return dataChunks.GetDataChunk(DataChunkName.NPC_MOVEMENT_LISTS); } }
        internal DataChunk NonPlayerCharacterMovementOffsets { get { return dataChunks.GetDataChunk(DataChunkName.NPC_MOVEMENT_OFFSETS); } }
        internal DataChunk NonPlayerCharacterKeySprites { get { return dataChunks.GetDataChunk(DataChunkName.NPC_SPRITE_INDEXES); } }
        internal DataChunk OverworldOverlayDataChunks { get { return overworldOverlayDataChunks.GetDataChunk(OverlayChunkName.CHARACTER_ANIMATION_STATES); } }
        internal DataChunk UnderworldOverlayDataChunks { get { return underworldOverlayDataChunks.GetDataChunk(OverlayChunkName.CHARACTER_ANIMATION_STATES); } }
        #endregion

        #region Public Properties
        public VirtualMap TheVirtualMap { get; set; }
        public TimeOfDay TheTimeOfDay { get; }


        public byte Gems
        {
            get
            {
                return dataChunks.GetDataChunk(DataChunkName.GEMS_QUANTITY).GetChunkAsByte();
            }
            set
            {
                dataChunks.GetDataChunk(DataChunkName.GEMS_QUANTITY).SetChunkAsByte(value);
            }
        }
        public byte Torches
        {
            get
            {
                return dataChunks.GetDataChunk(DataChunkName.TORCHES_QUANTITY).GetChunkAsByte();
            }
            set
            {
                dataChunks.GetDataChunk(DataChunkName.TORCHES_QUANTITY).SetChunkAsByte(value);

            }
        }
        public byte Keys
        {
            get
            {
                return dataChunks.GetDataChunk(DataChunkName.KEYS_QUANTITY).GetChunkAsByte();
            }
            set
            {
                dataChunks.GetDataChunk(DataChunkName.KEYS_QUANTITY).SetChunkAsByte(value);

            }
        }

        public SmallMapReferences.SingleMapReference.Location Location
        {
            get
            {
                return (SmallMapReferences.SingleMapReference.Location)dataChunks.GetDataChunk(DataChunkName.PARTY_LOC).GetChunkAsByte();
            }
        }        
        
        public int Floor
        {
            get
            {
                return dataChunks.GetDataChunk(DataChunkName.Z_COORD).GetChunkAsByte();
            }
        }

        public int X
        {
            get
            {
                return dataChunks.GetDataChunk(DataChunkName.X_COORD).GetChunkAsByte();
            }
        }

        public int Y
        {
            get
            {
                return dataChunks.GetDataChunk(DataChunkName.Y_COORD).GetChunkAsByte();
            }
        }



        public Inventory PlayerInventory
        {
            get;
        }

        public UInt16 Gold
        {
            get
            {
                return dataChunks.GetDataChunk(DataChunkName.GOLD_QUANTITY).GetChunkAsUINT16();
            }
            set
            {
                dataChunks.GetDataChunk(DataChunkName.GOLD_QUANTITY).SetChunkAsUINT16(value);

            }
        }

        public UInt16 Food
        {
            get
            {
                return dataChunks.GetDataChunk(DataChunkName.FOOD_QUANTITY).GetChunkAsUINT16();
            }
            set
            {
                dataChunks.GetDataChunk(DataChunkName.FOOD_QUANTITY).SetChunkAsUINT16(value);
            }
        }

        public bool HasGrapple
        {
            get; set;
        } = true;

        /// <summary>
        /// Users Karma
        /// </summary>
        public uint Karma { get; set; }

        /// <summary>
        /// The name of the Avatar
        /// </summary>
        public string AvatarsName { get { return CharacterRecords.Records[PlayerCharacterRecords.AVATAR_RECORD].Name; } }
        #endregion

        #region Enumerations
        public enum OverlayChunkName
        {
            Unused,
            CHARACTER_ANIMATION_STATES
        }
        /// <summary>
        /// Data chunks for each of the save game sections
        /// </summary>
        public enum DataChunkName
        {
            Unused,
            CHARACTER_RECORDS,
            NPC_ISALIVE_TABLE,
            NPC_ISMET_TABLE,
            N_PEOPLE_PARTY,
            FOOD_QUANTITY,
            GOLD_QUANTITY,
            KEYS_QUANTITY,
            GEMS_QUANTITY,
            TORCHES_QUANTITY,
            CURRENT_YEAR,
            CURRENT_MONTH,
            CURRENT_DAY,
            CURRENT_HOUR,
            CURRENT_MINUTE,
            NPC_TYPES,
            NPC_MOVEMENT_LISTS,
            NPC_MOVEMENT_OFFSETS,
            NPC_SPRITE_INDEXES,
            PARTY_LOC,
            Z_COORD,
            X_COORD,
            Y_COORD,
            CHARACTER_ANIMATION_STATES,
            CHARACTER_STATES,
        };
        #endregion



        #region Constructors
        public void InitializeVirtualMap(SmallMapReferences smallMapReferences, SmallMaps smallMaps,
            LargeMapReference largeMapReferences, LargeMap overworldMap, LargeMap underworldMap, NonPlayerCharacterReferences nonPlayerCharacters, 
            TileReferences TileReferences, GameState state, NonPlayerCharacterReferences npcRefs)
        {
            TheVirtualMap = new VirtualMap(smallMapReferences, smallMaps, largeMapReferences, overworldMap, underworldMap, 
                nonPlayerCharacters, TileReferences, state, npcRefs, TheTimeOfDay);
        }

        /// <summary>
        /// Construct the GameState
        /// </summary>
        /// <param name="u5Directory">Directory of the game State files</param>
        public GameState(string u5Directory, DataOvlReference dataOvlRef)
        {
            dataRef = dataOvlRef;

            string saveFileAndPath = Path.Combine(u5Directory, FileConstants.SAVED_GAM);

            dataChunks = new DataChunks<DataChunkName>(saveFileAndPath, DataChunkName.Unused);

            List<byte> gameStateByteArray;
            gameStateByteArray = Utils.GetFileAsByteList(saveFileAndPath);

            // import all character records
            dataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "All Character Records (ie. name, stats)", 0x02, 0x20*16, 0x00, DataChunkName.CHARACTER_RECORDS);
            DataChunk rawCharacterRecords = dataChunks.GetDataChunk(DataChunkName.CHARACTER_RECORDS);
            CharacterRecords = new PlayerCharacterRecords(rawCharacterRecords.GetAsByteList());

            // import the players invetry
            PlayerInventory = new Inventory(gameStateByteArray, dataRef);

            // player location
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Current Party Location", 0x2ED, 0x01, 0x00, DataChunkName.PARTY_LOC);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Z Coordinate of Party [10]", 0x2EF, 0x01, 0x00, DataChunkName.Z_COORD);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "X Coordinate of Party", 0x2F0, 0x01, 0x00, DataChunkName.X_COORD);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Y Coordinate of Party", 0x2F1, 0x01, 0x00, DataChunkName.Y_COORD);


            // quantities of standard items
            dataChunks.AddDataChunk(DataChunk.DataFormatType.UINT16, "Food Quantity", 0x202, 0x02, 0x00, DataChunkName.FOOD_QUANTITY);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.UINT16, "Gold Quantity", 0x204, 0x02, 0x00, DataChunkName.GOLD_QUANTITY);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Keys Quantity", 0x206, 0x01, 0x00, DataChunkName.KEYS_QUANTITY);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Gems Quantity", 0x207, 0x01, 0x00, DataChunkName.GEMS_QUANTITY);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Torches Quantity", 0x208, 0x01, 0x00, DataChunkName.TORCHES_QUANTITY);

            // time and date
            dataChunks.AddDataChunk(DataChunk.DataFormatType.UINT16, "Current Year", 0x2CE, 0x02, 0x00, DataChunkName.CURRENT_YEAR);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Current Month", 0x2D7, 0x01, 0x00, DataChunkName.CURRENT_MONTH);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Current Day", 0x2D8, 0x01, 0x00, DataChunkName.CURRENT_DAY);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Current Hour", 0x2D9, 0x01, 0x00, DataChunkName.CURRENT_HOUR);
            // 0x2DA is copy of 2D9 for some reason
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Byte, "Current Minute", 0x2DB, 0x01, 0x00, DataChunkName.CURRENT_MINUTE);


            //dataChunks.AddDataChunk()
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Bitmap, "NPC Killed Bitmap", 0x5B4, 0x80, 0x00, DataChunkName.NPC_ISALIVE_TABLE);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.Bitmap, "NPC Met Bitmap", 0x634, 0x80, 0x00, DataChunkName.NPC_ISMET_TABLE);
            dataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "Number of Party Members", 0x2B5, 0x1, 0x00, DataChunkName.N_PEOPLE_PARTY);

            dataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "NPC Type Map", 0x5B4, 0x20, 0x00, DataChunkName.NPC_TYPES);
            List<byte> chunks = dataChunks.GetDataChunk(DataChunkName.NPC_TYPES).GetAsByteList();

            // get the NPCs movement list - 0x20 NPCs, with 0x10 movement commands each, consisting of 0x1 direction byte + 0x1 repetitions
            dataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "NPC Movement List", 0xBB8, 0x20 * 0x10 * (sizeof(byte) * 2), 0x00, DataChunkName.NPC_MOVEMENT_LISTS);
            // bajh: Jan 12 2020, moved from BB8 to BBA to test a theory that it actually begins a few bytes after the original documentation indicates
            //dataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "NPC Movement List", 0xBBA, 0x20 * 0x10 * (sizeof(byte) * 2), 0x00, DataChunkName.NPC_MOVEMENT_LISTS);
            // get the offsets to the current movement instructions of the NPCs
            dataChunks.AddDataChunk(DataChunk.DataFormatType.UINT16List, "NPC Movement Offset Lists", 0xFB8, 0x20 * (sizeof(byte) * 2), 0x00, DataChunkName.NPC_MOVEMENT_OFFSETS);

            // we will need to add 0x100 for now, but cannot because it's read in as a bytelist
            dataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "NPC Sprite (by smallmap)", 0xFF8, 0x20, 0x00, DataChunkName.NPC_SPRITE_INDEXES);

            // Initialize the table to determine if an NPC is dead
            List<bool> npcAlive = dataChunks.GetDataChunk(DataChunkName.NPC_ISALIVE_TABLE).GetAsBitmapBoolList();
            npcIsDeadArray = Utils.ListTo2DArray<bool>(npcAlive, NonPlayerCharacterReferences.NPCS_PER_TOWN, 0x00, NonPlayerCharacterReferences.NPCS_PER_TOWN * SmallMapReferences.SingleMapReference.TOTAL_SMALL_MAP_LOCATIONS);

            // Initialize a table to determine if an NPC has been met
            List<bool> npcMet = dataChunks.GetDataChunk(DataChunkName.NPC_ISMET_TABLE).GetAsBitmapBoolList();
            // these will map directly to the towns and the NPC dialog #
            npcIsMetArray = Utils.ListTo2DArray<bool>(npcMet, NonPlayerCharacterReferences.NPCS_PER_TOWN, 0x00, NonPlayerCharacterReferences.NPCS_PER_TOWN * SmallMapReferences.SingleMapReference.TOTAL_SMALL_MAP_LOCATIONS);

            // this stores monsters, party, objects and NPC location info and other stuff too (apparently!?)
            dataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "Character Animation States - including xyz", 0x6B4, 0x100, 0x00, DataChunkName.CHARACTER_ANIMATION_STATES);

            // this stores monsters, party, objects and NPC location info and other stuff too (apparently!?)
            dataChunks.AddDataChunk(DataChunk.DataFormatType.UINT16List, "Character States - including xyz", 0x9B8, 0x200, 0x00, DataChunkName.CHARACTER_STATES);

            // load the overworld and underworld overlays
            string overworldOverlayPath = Path.Combine(u5Directory, FileConstants.BRIT_OOL);
            string underworldOverlayPath = Path.Combine(u5Directory, FileConstants.UNDER_OOL);

            overworldOverlayDataChunks = new DataChunks<OverlayChunkName>(overworldOverlayPath, OverlayChunkName.Unused);
            underworldOverlayDataChunks = new DataChunks<OverlayChunkName>(underworldOverlayPath, OverlayChunkName.Unused);

            overworldOverlayDataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "Character Animation States - including xyz", 0x00, 0x100, 0x00, OverlayChunkName.CHARACTER_ANIMATION_STATES);
            underworldOverlayDataChunks.AddDataChunk(DataChunk.DataFormatType.ByteList, "Character Animation States - including xyz", 0x00, 0x100, 0x00, OverlayChunkName.CHARACTER_ANIMATION_STATES);

            TheTimeOfDay = new TimeOfDay(dataChunks.GetDataChunk(DataChunkName.CURRENT_YEAR), dataChunks.GetDataChunk(DataChunkName.CURRENT_MONTH),
                dataChunks.GetDataChunk(DataChunkName.CURRENT_DAY), dataChunks.GetDataChunk(DataChunkName.CURRENT_HOUR), 
                dataChunks.GetDataChunk(DataChunkName.CURRENT_MINUTE));
        }
        #endregion

        #region Public Methods

        public void GrapplingFall()
        {
            // called when falling from a Klimb on a mountain
        }



        /// <summary>
        /// Using the random number generator, provides 1 in howMany odds of returning true
        /// </summary>
        /// <param name="howMany">1 in howMany odds of returning true</param>
        /// <returns>true if odds are beat</returns>
        public bool OneInXOdds(int howMany)
        {
            // if ran%howMany is zero then we beat the odds
            int nextRan = ran.Next();
            return ((nextRan % howMany) == 0);
        }

 
        /// <summary>
        /// Is NPC alive?
        /// </summary>
        /// <param name="npc">NPC object</param>
        /// <returns>true if NPC is alive</returns>
        public bool NpcIsAlive(NonPlayerCharacterReference npc)
        {
            // the array isDead becasue LB stores 0=alive, 1=dead
            // I think it's easier to evaluate if they are alive
            return npcIsDeadArray[npc.MapLocationID][npc.DialogIndex]==false;
        }

        /// <summary>
        /// Sets the flag to indicate the NPC is met
        /// </summary>
        /// <param name="npc"></param>
        public void SetMetNPC(NonPlayerCharacterReference npc)
        {
            npcIsMetArray[npc.MapLocationID][npc.DialogIndex] = true;
        }

        public int GetNumberOfActiveCharacters()
        {
            return GetActiveCharacterRecords().Count;
        }

    public List<PlayerCharacterRecord> GetActiveCharacterRecords()
    {
        List<PlayerCharacterRecord> activeCharacterRecords = new List<PlayerCharacterRecord>();

        foreach (PlayerCharacterRecord characterRecord in CharacterRecords.Records)
        {
            if (characterRecord.PartyStatus == PlayerCharacterRecord.CharacterPartyStatus.InParty)
                activeCharacterRecords.Add(characterRecord);
        }
        if (activeCharacterRecords.Count == 0) throw new Exception("Even the Avatar is dead, no records returned in active party");
        if (activeCharacterRecords.Count > PlayerCharacterRecords.MAX_PARTY_MEMBERS) throw new Exception("There are too many party members in the party... party...");

        return activeCharacterRecords;
    }

    public PlayerCharacterRecord GetCharacterFromParty(int nPosition)
        {
            Debug.Assert(nPosition >= 0 && nPosition < PlayerCharacterRecords.MAX_PARTY_MEMBERS, "There are a maximum of 6 characters");
            Debug.Assert(nPosition < CharacterRecords.TotalPartyMembers(), "You cannot request a character that isn't on the roster");

            int nPartyMember = 0;
            foreach (PlayerCharacterRecord characterRecord in CharacterRecords.Records)
            {
                if (characterRecord.PartyStatus == PlayerCharacterRecord.CharacterPartyStatus.InParty)
                    if (nPartyMember++ == nPosition) return characterRecord;
            }
            throw new Exception("I've asked for member of the party who is aparently not there...");
        }

        /// <summary>
        /// Adds an NPC character to the party, and maps their CharacterRecord
        /// </summary>
        /// <param name="npc">the NPC to add</param>
        public void AddMemberToParty(NonPlayerCharacterReference npc)
        {
            PlayerCharacterRecord record = CharacterRecords.GetCharacterRecordByNPC(npc);
            record.PartyStatus = PlayerCharacterRecord.CharacterPartyStatus.InParty;
        }

        /// <summary>
        /// Is my party full (at capacity)
        /// </summary>
        /// <returns>true if party is full</returns>
        public bool IsFullParty()
        {
            Debug.Assert(!(CharacterRecords.TotalPartyMembers() > PlayerCharacterRecords.MAX_PARTY_MEMBERS), "You have more party members than you should.");
            return (CharacterRecords.TotalPartyMembers() == PlayerCharacterRecords.MAX_PARTY_MEMBERS);
        }

        /// <summary>
        /// Has the NPC met the avatar yet?
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public bool NpcHasMetAvatar(NonPlayerCharacterReference npc)
        {
            return npcIsMetArray[npc.MapLocationID][npc.DialogIndex];
        }

        /// <summary>
        /// DEBUG FUNCTION
        /// </summary>
        /// <param name="npc"></param>
        public void SetNpcHasMetAvatar(NonPlayerCharacterReference npc, bool hasMet)
        {
            npcIsMetArray[npc.MapLocationID][npc.DialogIndex] = hasMet;
        }

        #endregion
    }
}
