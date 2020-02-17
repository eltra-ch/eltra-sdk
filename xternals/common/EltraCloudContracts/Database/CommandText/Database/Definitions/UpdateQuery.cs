/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

namespace TanamiBot.Database.CommandText.Database.Definitions
{
    public enum UpdateQuery
    {
        UpdateUndefined,
        UpdateHostInNodeUsingNodeId,
        UpdateHostStatusUsingHostId,
        UpdateHostErrorCountUsingHostId,
        UpdateIpLocationIdxUsingIpId,
        UpdateNodeStatusUsingNodeId,
        UpdateStatisticsProperty,
        UpdateIncreaseWordStatIdWeight,
        UpdateRegistrationStatus,
        UpdateRegistrationStatusUsingBotId,
        UpdateRegistrationBotPid,
        UpdateRegistrationBotStatus
    }
}
