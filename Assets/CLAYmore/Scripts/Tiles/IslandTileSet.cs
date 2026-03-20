using UnityEngine;
using UnityEngine.Tilemaps;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Island Tile Set", fileName = "IslandTileSet")]
    public class IslandTileSet : ScriptableObject
    {
        [Header("Light Tiles — Border")]
        public TileBase leftBotLight;
        public TileBase leftMidLight;
        public TileBase leftTopLight;
        public TileBase botLeftLight;
        public TileBase botCenterLight;
        public TileBase botRightLight;
        public TileBase rightBotLight;
        public TileBase rightMidLight;
        public TileBase rightTopLight;

        [Header("Light Tiles — Interior")]
        public TileBase interiorBotLeftCornerLight;
        public TileBase interiorBotCenterLight;
        public TileBase interiorBotRightCornerLight;
        public TileBase centerInnerLight;
        public TileBase interiorTopLeftCornerLight;
        public TileBase interiorTopCenterLight;
        public TileBase interiorTopRightCornerLight;

        [Header("Dark Tiles — Border")]
        public TileBase leftBotDark;
        public TileBase leftMidDark;
        public TileBase leftTopDark;
        public TileBase botLeftDark;
        public TileBase botCenterDark;
        public TileBase botRightDark;
        public TileBase rightBotDark;
        public TileBase rightMidDark;
        public TileBase rightTopDark;

        [Header("Dark Tiles — Interior")]
        public TileBase interiorBotLeftCornerDark;
        public TileBase interiorBotCenterDark;
        public TileBase interiorBotRightCornerDark;
        public TileBase centerInnerDark;
        public TileBase interiorTopLeftCornerDark;
        public TileBase interiorTopCenterDark;
        public TileBase interiorTopRightCornerDark;

        public TileBase GetTile(IslandTileType type, bool isLight)
        {
            if (isLight)
            {
                return type switch
                {
                    IslandTileType.LeftBot                 => leftBotLight,
                    IslandTileType.LeftMid                 => leftMidLight,
                    IslandTileType.LeftTop                 => leftTopLight,
                    IslandTileType.BotLeft                 => botLeftLight,
                    IslandTileType.BotCenter               => botCenterLight,
                    IslandTileType.BotRight                => botRightLight,
                    IslandTileType.RightBot                => rightBotLight,
                    IslandTileType.RightMid                => rightMidLight,
                    IslandTileType.RightTop                => rightTopLight,
                    IslandTileType.InteriorBotLeftCorner   => interiorBotLeftCornerLight,
                    IslandTileType.InteriorBotCenter       => interiorBotCenterLight,
                    IslandTileType.InteriorBotRightCorner  => interiorBotRightCornerLight,
                    IslandTileType.CenterInner             => centerInnerLight,
                    IslandTileType.InteriorTopLeftCorner   => interiorTopLeftCornerLight,
                    IslandTileType.InteriorTopCenter       => interiorTopCenterLight,
                    IslandTileType.InteriorTopRightCorner  => interiorTopRightCornerLight,
                    _                                      => null,
                };
            }
            else
            {
                return type switch
                {
                    IslandTileType.LeftBot                 => leftBotDark,
                    IslandTileType.LeftMid                 => leftMidDark,
                    IslandTileType.LeftTop                 => leftTopDark,
                    IslandTileType.BotLeft                 => botLeftDark,
                    IslandTileType.BotCenter               => botCenterDark,
                    IslandTileType.BotRight                => botRightDark,
                    IslandTileType.RightBot                => rightBotDark,
                    IslandTileType.RightMid                => rightMidDark,
                    IslandTileType.RightTop                => rightTopDark,
                    IslandTileType.InteriorBotLeftCorner   => interiorBotLeftCornerDark,
                    IslandTileType.InteriorBotCenter       => interiorBotCenterDark,
                    IslandTileType.InteriorBotRightCorner  => interiorBotRightCornerDark,
                    IslandTileType.CenterInner             => centerInnerDark,
                    IslandTileType.InteriorTopLeftCorner   => interiorTopLeftCornerDark,
                    IslandTileType.InteriorTopCenter       => interiorTopCenterDark,
                    IslandTileType.InteriorTopRightCorner  => interiorTopRightCornerDark,
                    _                                      => null,
                };
            }
        }
    }
}
