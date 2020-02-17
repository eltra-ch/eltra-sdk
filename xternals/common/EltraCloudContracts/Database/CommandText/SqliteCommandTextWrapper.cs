/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using TanamiBot.Database.CommandText.Database;
using TanamiBot.Database.CommandText.Database.Definitions;

namespace TanamiBot.Database.CommandText
{
    class SqliteCommandTextWrapper : DbCommandTextWrapper
    {
        public SqliteCommandTextWrapper(DbCommandText selection)
            : base(selection)
        {
        }

        protected override string GetSelectCommandText()
        {
            string result = string.Empty;

            switch (Query)
            {
                case SelectQuery.SelectGetNodeStatusUsingNodeId:
                    result = "select [status] from _node where md5=@param_md5";
                    break;
                case SelectQuery.SelectFindNodeUsingMd5:
                    result = "select node_id from _node where md5=@param_md5";
                    break;
                case SelectQuery.SelectGetNodesUsingStatus:
                    result = $"select node_id, url, host_idref, md5, modified from _node where [status]=@status {LimitText}";
                    break;
                case SelectQuery.SelectGetNodeEdgeIdUsingNodeFromTo:
                    result = "select node_edge_id from _node_edge where from_node_idref=@param_from and to_node_idref=@param_to";
                    break;
                case SelectQuery.SelectGetHostEdgeIdUsingHostFromTo:
                    result = "select host_edge_id from _host_edge where from_idref=@param_from and to_idref=@param_to";
                    break;
                case SelectQuery.SelectGetWordEdgeIdUsingNodeFromTo:
                    result = "select id_word_edge from _word_edge where from_word_idref=@from and to_word_idref=@to";
                    break;
                case SelectQuery.SelectFindNodeEdgeUsingNodeTo:
                    result = "select * from _node_edge where to_node_idref=@param_to";
                    break;
                case SelectQuery.SelectFindHostEdgeUsingHostTo:
                    result = "select * from _host_edge where to_idref=@param_to";
                    break;
                case SelectQuery.SelectFindNodeEdgeUsingNodeFrom:
                    result = "select * from _node_edge where from_node_idref=@param_from";
                    break;
                case SelectQuery.SelectFindHostEdgeUsingHostFrom:
                    result = "select * from _host_edge where from_idref=@param_from";
                    break;
                case SelectQuery.SelectGetNodeEdgesToUsingNodeFrom:
                    result = "select to_node_idref from _node_edge where from_node_idref=@param_from";
                    break;
                case SelectQuery.SelectGetHostEdgesToUsingHostFrom:
                    result = "select to_idref from _host_edge where from_idref=@param_from";
                    break;
                case SelectQuery.SelectGetParentNodeIdUsingNodeId:
                    result = "select from_node_idref from _node_edge where to_node_idref=@param_to";
                    break;
                case SelectQuery.SelectGetParentHostIdUsingHostId:
                    result = "select from_idref from _host_edge where to_idref=@param_to";
                    break;
                case SelectQuery.SelectGetNewUrlsUsingIp:
                    result = "select nn.nid, nn.url, nn.hidr, i.num_address" +
                    $" from((select n.node_id as nid, n.url as url, n.host_idref as hidr from _node as n where n.status = 1 {LimitText}) nn)" +
                    " inner join _ip as i on i.host_idref = nn.hidr" +
                    " where i.num_address is not null" +
                    " group by nn.nid";
                    break;
                case SelectQuery.SelectGetCountNewUrlsUsingIp:
                    result = "select count(*) from _node as n where n.status = 1";
                    break;
                case SelectQuery.SelectGetIpIdxUsingHostId:
                    result = "select [ip_id] from _ip where num_address=@num_address and host_idref=@hostId";
                    break;
                case SelectQuery.SelectFindHostUsingHostName:
                    result = "select host_id from _host where host_name=@host_name";
                    break;
                case SelectQuery.SelectGetHostListUsingStatus:
                    result = $"SELECT host_id, host_name, error_count, modified FROM _host where status=@status {LimitText}";
                    break;
                case SelectQuery.SelectGetHostNodeInfo:
                    result = $"select node_id, [url], host_idref from _node {LimitText}";
                    break;
                case SelectQuery.SelectGetLocationIdxUsingCoordinates:
                    result = "select location_id from _location where latitude=@latitude and longitude=@longitude";
                    break;
                case SelectQuery.SelectGetIpLocationIdxUsingIpId:
                    result = "select location_idref from _ip where ip_id=@ip_id";
                    break;
                case SelectQuery.SelectGetWordExceptions:
                    result = "select id_word_exception, tag, status, modified from _word_exception";
                    break;
                case SelectQuery.SelectGetWordIdUsingTag:
                    result = "select id_word from _word where tag = @tag";
                    break;
                case SelectQuery.SelectGetWordStatIdUsingWordId:
                    result = "select id_word_stat from _word_stat where node_idref = @node_id and word_idref = @word_id";
                    break;
                case SelectQuery.SelectGetDomainStatusUsingDomain:
                    result = "select status from _domain_filter where domain = @domain";
                    break;
                case SelectQuery.SelectGetWordPlaceIdxUsingTagWordId:
                    result = "select id_word_place from _word_place where word_idref=@wordId and word_place_region_idref=@regionId";
                    break;
                case SelectQuery.SelectGetAllWords:
                    result = $"SELECT w.id_word, w.tag, ws.node_idref FROM _word as w inner join _word_stat as ws on ws.word_idref=w.id_word {LimitText}";
                    break;
                case SelectQuery.SelectGetAllWordsWithNode:
                    result = $"SELECT w.id_word, w.tag, ws.node_idref FROM _word as w inner join _word_stat as ws on ws.word_idref=w.id_word {LimitText}";
                    break;
                case SelectQuery.SelectGetPlaceRegionIdxUsingRegion:
                    result = "select id_word_place_region from _word_place_region where region=@region and country=@country";
                    break;
                case SelectQuery.SelectGetAllDomainFilters:
                    result = "select domain, status from _domain_filter";
                    break;
                case SelectQuery.SelectGetWordExceptionUsingTag:
                    result = "select id_word_exception from _word_exception where tag=@tag";
                    break;
                case SelectQuery.SelectFindMetaNodeUsingName:
                    result = "select id_node_meta from _node_meta where node_idref=@nodeId and name=@key";
                    break;
                case SelectQuery.SelectGetAllNodeMetaUsingName:
                    result = $"select * from _node_meta where name=@name {LimitText}";
                    break;
                case SelectQuery.SelectGetNodesWithRevisitAfter:
                    result = "select n.node_id, nm.value, n.modified from _node_meta as nm " +
                             "inner join _node as n on n.node_id = nm.node_idref " +
                             "where nm.name = 'revisit-after' and n.status = @status " +
                             $"order by nm.id_node_meta desc {LimitText}";
                    break;
                case SelectQuery.SelectFindNodeHostIdUsingNodeId:
                    result = "select host_idref from _node where node_id=@nodeId";
                    break;
                case SelectQuery.SelectGetAllNodeHostEdges:
                    result = "select n1.host_idref as from_host_id, n2.host_idref as to_host_id " +
                             "from _node_edge as e " +
                             "inner join _node as n1 " +
                             "on n1.node_id = e.from_node_idref " +
                             "inner join _node as n2 " +
                             "on n2.node_id = e.to_node_idref " +
                             $"where n1.host_idref<> n2.host_idref {LimitText}";
                    break;
                case SelectQuery.SelectStatisticsLastModified:
                    result = "select modified from _statistics order by id_statistics desc limit 1";
                    break;
                case SelectQuery.SelectStatisticsLastHosts:
                    result = "select count(*) from _host as h where h.modified >= DATE_SUB(NOW(), INTERVAL 24 hour)";
                    break;
            }

            return result;
        }

        protected override string GetUpdateCommandText()
        {
            string result = string.Empty;

            switch (CommandTextUpdating)
            {
                case UpdateQuery.UpdateNodeStatusUsingNodeId:
                    result = "update _node set [status]=@param_status, modified=datetime('now','localtime') where node_id=@param_index";
                    break;
                case UpdateQuery.UpdateHostInNodeUsingNodeId:
                    result = "update _node set host_idref = @hostid where node_id=@nodeid";
                    break;
                case UpdateQuery.UpdateHostStatusUsingHostId:
                    result = "update _host set status = @status where host_id=@hostid";
                    break;
                case UpdateQuery.UpdateHostErrorCountUsingHostId:
                    result = "update _host set status = @status, error_count=@error_count where host_id=@hostid";
                    break;
                case UpdateQuery.UpdateIpLocationIdxUsingIpId:
                    result = "update _ip set location_idref=@location_idref where ip_id=@ip_id";
                    break;
            }

            return result;
        }

        protected override string GetDeleteCommandText()
        {
            string result = string.Empty;

            switch (CommandTextDelete)
            {
                case DeleteQuery.DeleteWord:
                    result = "delete from _word where id_word=@wordId";
                    break;
                case DeleteQuery.DeleteWordPlace:
                    result = "delete from _word_place where word_idref=@wordId";
                    break;
                case DeleteQuery.DeleteWordStat:
                    result = "delete from _word_stat where word_idref=@wordId";
                    break;
            }

            return result;
        }

        protected override string GetInsertCommandText()
        {
            string result = string.Empty;

            switch (CommandTextInsertion)
            {
                case InsertQuery.InsertNode:
                    result = "insert into _node (url, host_idref, type, md5, modified) values (@url, @host_idref, @type, @md5, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertNodeEdge:
                    result = "insert into _node_edge (from_node_idref, to_node_idref, modified) values (@param_from, @param_to, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertHostEdge:
                    result = "insert into _host_edge (from_idref, to_idref, modified) values (@param_from, @param_to, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertWordEdge:
                    result = "insert into _word_edge (from_word_idref, to_word_idref, modified) values (@from, @to, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertIp:
                    result = "insert into _ip (num_address, host_idref, location_idref, modified) values (@num_address, @host_idref, @location_idref, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertHost:
                    result = "insert into _host (host_name, status, modified) values (@host_name, 0, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertLocation:
                    result = "insert into _location (country_code, country, region, city, latitude, longitude, modified) values "
                             + "(@countrycode, @country, @region, @city, @latitude, @longitude, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertWord:
                    result = "insert into _word (tag) values(@tag)";
                    break;
                case InsertQuery.InsertWordStat:
                    result = "insert into _word_stat (node_idref, word_idref, node_meta_idref) values(@node_id, @word_id, @meta_id)";
                    break;
                case InsertQuery.InsertWordPlace:
                    result = "insert into _word_place (tag, word_place_region_idref, word_idref, modified) values(@tag, @regionId, @wordId, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertWordPlaceRegion:
                    result = "insert into _word_place_region (region, country) values(@region, @country)";
                    break;
                case InsertQuery.InsertWordException:
                    result = "insert into _word_exception (tag, status, modified) values(@tag, @status, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertMetaTag:
                    result = "insert into _node_meta (node_idref, name, value, modified) values(@nodeId, @key, @value, datetime('now','localtime'))";
                    break;
            }

            return result;
        }
    }
}
