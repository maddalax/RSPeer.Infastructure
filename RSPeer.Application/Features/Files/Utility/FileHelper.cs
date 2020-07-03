using System;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Files.Utility
{
    public class FileHelper
    {
        public static string GetFileName(Game game)
        {
            switch (game)
            {
                case Game.Osrs:
                    return "rspeer.jar";
                case Game.Rs3:
                    return "inuvation.jar";
                case Game.Rs3Updater:
                    return "inuvation-updater.jar";
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }
    }
}