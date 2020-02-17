/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

namespace TanamiBot.Database.CommandText.Database.Definitions
{
    public enum SelectQuery
    {
        SelectUndefined,
        SelectGetNodeStatusUsingNodeId,
        SelectGetOwnedNodeStatus,
        SelectGetNodeIdStatusUsingMd5,
        SelectFindNodeUsingMd5,
        SelectGetNodeEdgeIdUsingNodeFromTo,
        SelectFindNodeEdgeUsingNodeTo,
        SelectFindNodeEdgeUsingNodeFrom,
        SelectGetNodeEdgesToUsingNodeFrom,
        SelectGetParentNodeIdUsingNodeId,
        SelectGetNewUrlsUsingIp,
        SelectGetCountNewUrlsUsingIp,
        SelectGetCountNewUrls,
        SelectGetIpIdxUsingHostId,
        SelectFindHostUsingHostName,
        SelectGetHostListUsingStatus,
        SelectGetHostNodeInfo,
        SelectGetNodesUsingStatus,
        SelectGetLocationIdxUsingCoordinates,
        SelectGetIpLocationIdxUsingIpId,
        SelectGetWordExceptions,
        SelectGetWordIdUsingTag,
        SelectGetWordStatIdUsingWordId,
        SelectGetDomainStatusUsingDomain,
        SelectGetWordPlaceIdxUsingTagWordId,
        SelectGetAllWords,
        SelectGetAllWordsWithNode,
        SelectGetPlaceRegionIdxUsingRegion,
        SelectGetAllDomainFilters,
        SelectGetWordExceptionUsingTag,
        SelectGetWordEdgeIdUsingNodeFromTo,
        SelectFindMetaNodeUsingName,
        SelectGetAllNodeMetaUsingName,
        SelectGetNodesWithRevisitAfter,
        SelectGetHostEdgeIdUsingHostFromTo,
        SelectFindHostEdgeUsingHostTo,
        SelectFindHostEdgeUsingHostFrom,
        SelectGetHostEdgesToUsingHostFrom,
        SelectGetParentHostIdUsingHostId,
        SelectFindNodeHostIdUsingNodeId,
        SelectGetAllNodeHostEdges,
        SelectStatisticsLastModified,
        SelectStatisticsLastHosts,
        SelectStatisticsProperty,
        SelectStatisticsInt32PropertyValue,
        SelectStatisticsLastNodes,
        SelectRegistrationBotId,
        SelectRegistrationStatusByName,
        SelectRegistrationStatusByProcessInfo,
        SelectRegistrationStatusById,
        SelectRegistrationBotsByName,
        SelectRegistrationGetAllBots,
        SelectGetOpenXmlNodes,
        SelectNodesWithSlashOnTheEnd
    }
}
