/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using TanamiBot.Database.CommandText.Database;
using TanamiBot.Database.CommandText.Database.Definitions;

namespace TanamiBot.Database.CommandText
{
    class MySqlCommandTextWrapper : DbCommandTextWrapper
    {
        public MySqlCommandTextWrapper(DbCommandText selection)
            : base(selection)
        {
        }
        
        protected override string GetSelectCommandText()
        {
            string result = string.Empty;
            
            switch (Query)
            {
                case SelectQuery.SelectGetNodeStatusUsingNodeId:
                    result = "select status from _node where node_id=@nodeId";
                    break;
                case SelectQuery.SelectGetOwnedNodeStatus:
                    result = "select status from _node where node_id=@nodeId and bot_idref=@botId";
                    break;
                case SelectQuery.SelectGetNodeIdStatusUsingMd5:
                    result = "select node_id,status from _node where md5=@md5";
                    break;
                case SelectQuery.SelectFindNodeUsingMd5:
                    result = "select node_id from _node where md5=@param_md5";
                    break;
                case SelectQuery.SelectGetNodesUsingStatus:
                    result = $"select node_id,url,host_idref,md5,modified from _node where status=@status {LimitText}";
                    break;
                case SelectQuery.SelectGetOpenXmlNodes:
                    result = $"select node_id,url,host_idref from _node where url like \"%.docx\" and status=@status {LimitText}";
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
                    /*result = "select * from (select nn.nid, nn.url, nn.hidr, i.num_address from " +
                            $"((select n.node_id as nid, n.url as url, n.host_idref as hidr from _node as n where n.status = @status order by n.node_id {LimitText}) nn) " +
                            "inner join _ip as i on i.host_idref = nn.hidr " +
                            "where i.num_address is not null " +
                            "group by i.num_address " +
                            "order by nid) as f " +
                            "group by hidr; ";*/
                    /*result = "select n.node_id, n.url, n.host_idref, i.num_address from _node as n " +
                                "inner join _host as h on h.host_id = n.host_idref " +
                                $"inner join _ip as i on h.host_id = i.host_idref where i.num_address is not null and n.status=@status1 or n.status=@status2 or n.status=@status3 {LimitText}";*/
                                
                    result = "select n.node_id, n.url, n.host_idref, i.num_address, n.status from " +
                             "(select* from _node where status=@status1 or status=@status2 or status=@status3) as n " +
                             "inner join _host as h on h.host_id = n.host_idref " +
                             "inner join _ip as i on h.host_id = i.host_idref and i.num_address is not null " +
                             "group by i.num_address " + $"{LimitText}";

                    break;
                case SelectQuery.SelectGetCountNewUrls:
                    result = "select s.value from _statistics as s where s.property='new_nodes_count'";
                    break;
                case SelectQuery.SelectGetCountNewUrlsUsingIp:

                    string subQuery = "select * from (select nn.nid, nn.url, nn.hidr, i.num_address from " +
                                        $"((select n.node_id as nid, n.url as url, n.host_idref as hidr from _node as n where n.status = @status order by n.node_id {LimitText}) nn) " +
                                        "inner join _ip as i on i.host_idref = nn.hidr " +
                                        "where i.num_address is not null " +
                                        "group by i.num_address " +
                                        "order by nid) as f " +
                                        "group by hidr";

                    result = $"select count(*) from ({subQuery}) as gc";

                    break;
                case SelectQuery.SelectGetIpIdxUsingHostId:
                    result = "select ip_id from _ip where num_address=@num_address and host_idref=@hostId";
                    break;
                case SelectQuery.SelectFindHostUsingHostName:
                    result = "select host_id from _host where host_name=@host_name";
                    break;
                case SelectQuery.SelectGetHostListUsingStatus:
                    result = $"SELECT host_id, host_name, error_count, modified FROM _host where status=@status {LimitText}";
                    break;
                case SelectQuery.SelectGetHostNodeInfo:
                    result = $"select node_id, url, host_idref from _node {LimitText}";
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
                    result = "select id_word_stat from _word_stat where node_idref = @node_id and word_edge_idref = @word_edge_id";
                    break;
                case SelectQuery.SelectGetDomainStatusUsingDomain:
                    result = "select status from _domain_filter where domain = @domain";
                    break;
                case SelectQuery.SelectGetWordPlaceIdxUsingTagWordId:
                    result = "select id_word_place from _word_place where word_idref=@wordId and word_place_region_idref=@regionId";
                    break;
                case SelectQuery.SelectGetAllWords:
                    result = $"SELECT id_word, tag FROM _word {LimitText}";
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
                case SelectQuery.SelectNodesWithSlashOnTheEnd:
                    result = $"select * from _node where url like '%/' {LimitText}";
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
                                     $"where n1.host_idref <> n2.host_idref {LimitText}";
                    break;
                case SelectQuery.SelectStatisticsLastModified:
                    result = "select modified from _statistics as s where s.property='hosts_count_24h' or s.property='new_nodes_count_24h' order by s.modified desc limit 1";
                    break;
                case SelectQuery.SelectStatisticsLastHosts:
                    result = $"select host_id, modified from _host as h order by h.host_id desc {LimitText}";
                    break;
                case SelectQuery.SelectStatisticsProperty:
                    result = "select id_statistics from _statistics where property=@property";
                    break;
                case SelectQuery.SelectStatisticsInt32PropertyValue:
                    result = "select s.value from _statistics as s where s.property=@propertyName";
                    break;
                case SelectQuery.SelectStatisticsLastNodes:
                    result = "select count(*) as p24h from _node as n where n.status = @status and n.modified >= DATE_SUB(NOW(), INTERVAL 24 hour)";
                    break;
                case SelectQuery.SelectRegistrationBotId:
                    result = "select bot_id from _bot where name=@name and pid=@pid and hostname=@hostname";
                    break;
                case SelectQuery.SelectRegistrationStatusByName:
                    result = "select status from _bot where name=@name";
                    break;
                case SelectQuery.SelectRegistrationStatusById:
                    result = "select status from _bot where bot_id=@botId";
                    break;
                case SelectQuery.SelectRegistrationStatusByProcessInfo:
                    result = "select status from _bot where name=@name and pid=@pid and hostname=@hostname";
                    break;
                case SelectQuery.SelectRegistrationBotsByName:
                    result = "select bot_id, name, pid, hostname, status, modified from _bot where name=@name";
                    break;
                case SelectQuery.SelectRegistrationGetAllBots:
                    result = "select b.bot_id, b.name, b.pid, b.hostname, b.status, lastchange from " +
                                   "(select n.bot_idref, max(n.modified) as lastchange from _node as n where n.bot_idref > 0 " +
                                   "group by n.bot_idref " +
                                   "order by n.modified desc) as bs " +
                                   "inner join _bot as b " +
                                   "on b.bot_id = bs.bot_idref";
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
                    result = "update _node set bot_idref=@botId, status=@status, modified=NOW() where node_id=@nodeId";
                    break;
                case UpdateQuery.UpdateHostInNodeUsingNodeId:
                    result = "update _node set host_idref = @hostid, modified=NOW() where node_id=@nodeid";
                    break;
                case UpdateQuery.UpdateHostStatusUsingHostId:
                    result = "update _host set status = @status, modified=NOW() where host_id=@hostid";
                    break;
                case UpdateQuery.UpdateHostErrorCountUsingHostId:
                    result = "update _host set status = @status, error_count=@error_count, modified=NOW() where host_id=@hostid";
                    break;
                case UpdateQuery.UpdateIpLocationIdxUsingIpId:
                    result = "update _ip set location_idref=@location_idref, modified=NOW() where ip_id=@ip_id";
                    break;
                case UpdateQuery.UpdateStatisticsProperty:
                    result = "update _statistics set value=@value, modified=NOW() where id_statistics=@propertyId";
                    break;
                case UpdateQuery.UpdateIncreaseWordStatIdWeight:
                    result = "update _word_stat set weight=weight+1, modified=NOW() where id_word_stat=@wordStatId";
                    break;
                case UpdateQuery.UpdateRegistrationStatus:
                    result = "update _bot set status=@status,modified=NOW() where name=@name and pid=@pid and hostname=@hostname";
                    break;
                case UpdateQuery.UpdateRegistrationBotPid:
                    result = "update _bot set pid=@pid,hostname=@hostname,modified=NOW() where bot_id=@botId";
                    break;
                case UpdateQuery.UpdateRegistrationBotStatus:
                    result = "update _bot set status=@status,modified=NOW() where bot_id=@botId";
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
                    result = "insert into _node (url, bot_idref, host_idref, type, status, md5, modified) values (@url, @bot_idref, @host_idref, @type, 0, @md5, NOW())";
                    break;
                case InsertQuery.InsertNodeEdge:
                    result = "insert into _node_edge (from_node_idref, to_node_idref, modified) values (@param_from, @param_to, NOW())";
                    break;
                case InsertQuery.InsertHostEdge:
                    result = "insert into _host_edge (from_idref, to_idref, modified) values (@param_from, @param_to, NOW())";
                    break;
                case InsertQuery.InsertWordEdge:
                    result = "insert into _word_edge (from_word_idref, to_word_idref, modified) values (@from, @to, NOW())";
                    break;
                case InsertQuery.InsertIp:
                    result = "insert into _ip (num_address, host_idref, location_idref, modified) values (@num_address, @host_idref, @location_idref, NOW())";
                    break;
                case InsertQuery.InsertHost:
                    result = "insert into _host (host_name, status, modified) values (@host_name, 0, NOW())";
                    break;
                case InsertQuery.InsertLocation:
                    result = "insert into _location (country_code, country, region, city, latitude, longitude, modified) values "
                                  + "(@countrycode, @country, @region, @city, @latitude, @longitude, NOW())";
                    break;
                case InsertQuery.InsertWord:
                    result = "insert into _word (tag) values(@tag)";
                    break;
                case InsertQuery.InsertWordStat:
                    result = "insert into _word_stat (node_idref, word_edge_idref, node_meta_idref) values(@node_id, @word_edge_id, @meta_id)";
                    break;
                case InsertQuery.InsertWordPlace:
                    result = "insert into _word_place (tag, word_place_region_idref, word_idref, modified) values(@tag, @regionId, @wordId, NOW())";
                    break;
                case InsertQuery.InsertWordPlaceRegion:
                    result = "insert into _word_place_region (region, country) values(@region, @country)";
                    break;
                case InsertQuery.InsertWordException:
                    result = "insert into _word_exception (tag, status, modified) values(@tag, @status, NOW())";
                    break;
                case InsertQuery.InsertMetaTag:
                    result = "insert into _node_meta (node_idref, name, value, modified) values(@nodeId, @key, @value, NOW())";
                    break;
                case InsertQuery.InsertStatisticsProperty:
                    result = "insert into _statistics (property,value) values (@property, @value)";
                    break;
                case InsertQuery.InsertRegistrationRegister:
                    result = "insert into _bot (name,pid,hostname) values (@name, @pid,@hostname)";
                    break;
            }

            return result;
        }
    }
}
