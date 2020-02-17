/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

namespace EltraCloudStorage.DataSource.CommandText.Database.Definitions
{
    public enum UpdateQuery
    {
        UpdateUndefined,
        UpdateDeviceStatus,
        UpdateDeviceUserStatus,
        UpdateSessionStatus,
        UpdateSessionDeviceStatus,
        UpdateSessionCommand,
        UpdateExecCommandStatus,
        UpdateExecCommandCommStatus,
        UpdateExecCommandParameterValue,
        UpdateDataType,
        UpdateSessionStatusById,
        UpdateLocationStatus,
        UpdateUserStatus,
        UpdateUserStatusByToken,
        UpdateTool,
        UpdateToolSet,
        UpdateDeviceDescription,
        SetSessionLinkStatus,
        SetSessionLinkStatusBySessionId
    }
}
