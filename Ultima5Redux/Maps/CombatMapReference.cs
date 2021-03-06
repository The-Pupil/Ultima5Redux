﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultima5Redux
{
    public class CombatMapReference
    {
        public class SingleCombatMapReference
        {
            /// <summary>
            /// Create the reference based on territory and a map number 
            /// </summary>
            /// <param name="mapTerritory">Britannia or Dungeon</param>
            /// <param name="combatMapNum">map number in data file (0,1,2,3....)</param>
            public SingleCombatMapReference(Territory mapTerritory, short combatMapNum)
            {
                MapTerritory = mapTerritory;
                CombatMapNum = combatMapNum;
            }

            /// Descriptions of each combat map
            private static readonly string[] britanniaDescriptions = { "Camp Fire", "Swamp", "Glade", "Treed", "Desert", "Clean Tree", "Mountains", "Big Bridge", "Brick", "Basement", "Psychodelic",
                "Boat - Ocean", "Boat - North", "Boat - South", "Boat-Boat", "Bay"};
            private static readonly string[] dungeonDescriptions = { "A", "B" };

            /// <summary>
            /// How many bytes for each combat map entry in data file
            /// </summary>
            public const int MAP_BYTE_COUNT = 0x0160;

            /// <summary>
            /// Offset of combat map in data file
            /// </summary>
            public int FileOffset
            {
                get
                {
                    return (CombatMapNum * MAP_BYTE_COUNT);
                }
            }

            /// <summary>
            /// The territory that the combat map is in. This matters most for determing data files.
            /// </summary>
            public enum Territory { Britannia = 0, Dungeon };

            /// <summary>
            /// The number of the combat map (order in data file)
            /// </summary>
            public short CombatMapNum { get; }

            /// <summary>
            /// Brief description of the combat map
            /// </summary>
            public string Description
            {
                get
                {
                    if (MapTerritory==Territory.Britannia)
                    {
                        return britanniaDescriptions[CombatMapNum];
                    }
                    else
                    {
                        return dungeonDescriptions[CombatMapNum];
                    }
                }
            }
            
            /// <summary>
            /// Territory of the combat map
            /// </summary>
            public Territory MapTerritory { get; set; }

            /// <summary>
            /// Generated 
            /// </summary>
            /// <remarks>this needs to rewritten when we understand how the data files refer to Combat Maps</remarks>
            public byte Id
            {
                get
                {
                    return (byte)MapTerritory;
                }
            }

            /// <summary>
            /// Filename of the resource based on it's Territory
            /// </summary>
            public string MapFilename
            {
                get
                {
                    switch (MapTerritory)
                    {
                        case Territory.Britannia:
                            return "brit.cbt";
                        case Territory.Dungeon:
                            return "dungeon.cbt";
                    }
                    return "";
                }
            }

        }

        /// <summary>
        /// Build the combat map reference 
        /// </summary>
        public CombatMapReference()
        {
            for (short i=0; i < 16; i++)
            {
                mapReferences.Add(new SingleCombatMapReference(SingleCombatMapReference.Territory.Britannia, i));
            }
        }

        // the master copy of the map references
        private List<SingleCombatMapReference> mapReferences = new List<SingleCombatMapReference>();

        /// <summary>
        /// The list of map references
        /// </summary>
        public List<SingleCombatMapReference> MapReferenceList
        {
            get
            {
                return mapReferences;
            }
        }

    }
}
