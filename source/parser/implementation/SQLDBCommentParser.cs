using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using MyDocGenerator.source.parser.interfaces;
using MyDocGenerator.source.kernel;
using System.Data.SqlClient;
using ExcelUtilityLibrary;

namespace MyDocGenerator.source.parser.implementation
{
    class SQLDBCommentParser : ICommentParser
    {

        private ExcelUtility dbUtility;

        protected List<CommentNode> nodes;

        public SQLDBCommentParser(){
            nodes = new List<CommentNode>();
        }

        public void loadConfiguration(string fpath) {
            if (dbUtility != null)
                dbUtility.terminateDBConnection();
            dbUtility = new ExcelUtility();
            //parse config file 
            loadDBConfigFile(fpath);
            
            //connect to database
        }

        /// <summary>
        /// Loads a configuration file for database parameters
        /// </summary>
        /// <param name="path">Path to XML file with db parameters</param>
        private void loadDBConfigFile(string path)
        {
            try
            {
                string db = null;
                string server = null;
                string usr = null;
                string pwd = null;
                using (XmlReader reader = new XmlTextReader(path))
                {
                    while (reader.Read())
                    {
                        if (reader.ReadToFollowing("server"))
                            server = reader.ReadElementContentAsString();
                        if (reader.ReadToFollowing("dbcatalog"))
                            db = reader.ReadElementContentAsString();
                        if (reader.ReadToFollowing("user"))
                            usr = reader.ReadElementContentAsString();
                        if (reader.ReadToFollowing("pwd"))
                            pwd = reader.ReadElementContentAsString();
                    }
                    reader.Close();
                }
                if (!(String.IsNullOrEmpty(db) || String.IsNullOrEmpty(server) || String.IsNullOrEmpty(usr) || String.IsNullOrEmpty(pwd)))
                {
                    //connect to database
                    dbUtility.connectToDB(server, db, usr, pwd);
                }

                else
                    throw new Exception("Empty or invalid values for connecting to the database");
            }
            catch (Exception e)
            {

                Console.WriteLine("Error connecting db: {0}", e.Message);
                throw e;
            }
        } 

        public void parseFile(string fpath) {
            
        }

        public List<CommentNode> getCommentNodes() {
            return nodes;
        }

        public void parse() { 
            //parse according to new parameters
            //parse tables
            //parse constraints/keys
            //parse functions
            //parse procedures
            parseTables();
            parseConstraints();
            parseFunctions();
            parseStoredProcedures();
            parseViews();
            parseTableTypes();
            parseIndexes();
        }

        /// <summary>
        /// creates tags for tables on the database
        /// </summary>
        private void parseTables() {
            //get tables
            string query = "select o.object_id, o.name from sys.objects o where o.type = @type ";
            using (SqlDataReader tables = dbUtility.executeQuery(query, new SqlParameter("@type", "U"))) {
                if(tables.HasRows){
                    while(tables.Read()){
                        CommentNode n = new CommentNode();
                        CommentTag t = new CommentTag("table", tables[1].ToString());
                        n.addTag(t);
                        //add columns as tags
                        query = "select c.name, t.name, c.max_length from sys.columns c inner join sys.types t on c.user_type_id = t.user_type_id " + 
                            "where c.object_id = @oid";
                        using (SqlDataReader cols = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0)))) {
                            if (cols.HasRows) {
                                while (cols.Read()) {
                                    string col = String.Format("[{0}] {1}({2})", cols[0], cols[1], cols[2]);
                                    n.addTag(new CommentTag("column", col));
                                }
                            }
                        }
                        //add constraints as tags
                        query = "select k.name, k.type from sys.key_constraints k where k.parent_object_id = @oid";
                        using (SqlDataReader cons = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (cons.HasRows)
                            {
                                while (cons.Read())
                                {
                                    string con = String.Format("{0}", cons[0]);
                                    n.addTag(new CommentTag("keys/constraints", con));
                                }
                            }
                        }
                        query = "select k.name, k.type from sys.check_constraints k where k.parent_object_id = @oid";
                        using (SqlDataReader cons = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (cons.HasRows)
                            {
                                while (cons.Read())
                                {
                                    string con = String.Format("{0}", cons[0]);
                                    n.addTag(new CommentTag("keys/constraints", con));
                                }
                            }
                        }
                        query = "select k.name, k.type from sys.default_constraints k where k.parent_object_id = @oid";
                        using (SqlDataReader cons = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (cons.HasRows)
                            {
                                while (cons.Read())
                                {
                                    string con = String.Format("{0}", cons[0]);
                                    n.addTag(new CommentTag("keys/constraints", con));
                                }
                            }
                        }
                        //add foreign keys as tags
                        query = "select k.name, o.name from sys.foreign_keys k inner join sys.objects o on k.referenced_object_id = o.object_id where k.parent_object_id = @oid";
                        using (SqlDataReader foreign = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (foreign.HasRows)
                            {
                                while (foreign.Read())
                                {
                                    string con = String.Format("{0} references {1}", foreign[0], foreign[1]);
                                    n.addTag(new CommentTag("foreign key", con));
                                }
                            }
                        }

                        //add new node
                        nodes.Add(n);
                    }
                    
                }
                
            }
            // get 
        }

        /// <summary>
        /// creates tags for tables on the database
        /// </summary>
        private void parseTableTypes()
        {
            //get tables
            string query = "select o.object_id, t.name from sys.objects o inner join sys.table_types t on o.object_id = t.type_table_object_id where o.type = @type";
            using (SqlDataReader tables = dbUtility.executeQuery(query, new SqlParameter("@type", "TT")))
            {
                if (tables.HasRows)
                {
                    while (tables.Read())
                    {
                        CommentNode n = new CommentNode();
                        CommentTag t = new CommentTag("table type", tables[1].ToString());
                        n.addTag(t);
                        //add columns as tags
                        query = "select c.name, t.name, c.max_length from sys.columns c inner join sys.types t on c.user_type_id = t.user_type_id " +
                            "where c.object_id = @oid";
                        using (SqlDataReader cols = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (cols.HasRows)
                            {
                                while (cols.Read())
                                {
                                    string col = String.Format("[{0}] {1}({2})", cols[0], cols[1], cols[2]);
                                    n.addTag(new CommentTag("column", col));
                                }
                            }
                        }
                        //add constraints as tags
                        query = "select k.name, k.type from sys.key_constraints k where k.parent_object_id = @oid";
                        using (SqlDataReader cons = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (cons.HasRows)
                            {
                                while (cons.Read())
                                {
                                    string con = String.Format("{0}", cons[0]);
                                    n.addTag(new CommentTag("keys/constraints", con));
                                }
                            }
                        }
                        query = "select k.name, k.type from sys.check_constraints k where k.parent_object_id = @oid";
                        using (SqlDataReader cons = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (cons.HasRows)
                            {
                                while (cons.Read())
                                {
                                    string con = String.Format("{0}", cons[0]);
                                    n.addTag(new CommentTag("keys/constraints", con));
                                }
                            }
                        }
                        query = "select k.name, k.type from sys.default_constraints k where k.parent_object_id = @oid";
                        using (SqlDataReader cons = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (cons.HasRows)
                            {
                                while (cons.Read())
                                {
                                    string con = String.Format("{0}", cons[0]);
                                    n.addTag(new CommentTag("keys/constraints", con));
                                }
                            }
                        }
                        //add foreign keys as tags
                        query = "select k.name, o.name from sys.foreign_keys k inner join sys.objects o on k.referenced_object_id = o.object_id where k.parent_object_id = @oid";
                        using (SqlDataReader foreign = dbUtility.executeQuery(query, new SqlParameter("@oid", tables.GetInt32(0))))
                        {
                            if (foreign.HasRows)
                            {
                                while (foreign.Read())
                                {
                                    string con = String.Format("{0} references {1}", foreign[0], foreign[1]);
                                    n.addTag(new CommentTag("foreign key", con));
                                }
                            }
                        }

                        //add new node
                        nodes.Add(n);
                    }

                }

            }
            // get 
        }
        /// <summary>
        /// Parses Indexes
        /// </summary>
        private void parseIndexes() {
            //get indexes
            string query = "select i.object_id, i.index_id, i.name, t.name from sys.indexes i "+
	                        "inner join sys.tables t "+
	                        "on i.object_id = t.object_id "+
	                        "where i.is_primary_key = 0 "+
                            "AND i.is_unique = 0 "+
                            "AND i.is_unique_constraint = 0 "+
                            "and t.is_ms_shipped = 0 and i.name is not null";
            using (SqlDataReader indexes = dbUtility.executeQuery(query))
            {
                if (indexes.HasRows)
                {
                    while (indexes.Read())
                    {
                        int objID = indexes.GetInt32(0);
                        int indexID = indexes.GetInt32(1);
                        CommentNode n = new CommentNode();
                        CommentTag t = new CommentTag("index", indexes[2].ToString());
                        n.addTag(t);
                        //add columns as tags
                        query = "select c.name, t.name, c.max_length "+
                                "from sys.index_columns i "+
	                                "inner join "+
	                                "sys.columns c "+
	                                "on i.object_id = c.object_id and i.column_id = c.column_id "+
	                                "inner join "+
	                                "sys.types t "+
	                                "on c.user_type_id = t.user_type_id " +
                                    "where i.object_id = @oid and i.index_id = @inID";
                        using (SqlDataReader cols = dbUtility.executeQuery(query, new SqlParameter("@oid", objID), new SqlParameter("@inID", indexID)))
                        {
                            if (cols.HasRows)
                            {
                                while (cols.Read())
                                {
                                    string col = String.Format("[{0}] {1}({2})", cols[0], cols[1], cols[2]);
                                    n.addTag(new CommentTag("column", col));
                                }
                            }
                        }
                        //add table as tags
                        n.addTag(new CommentTag("table", String.Format("{0}", indexes[3])));
                        //add new node
                        nodes.Add(n);
                    }

                }

            }
            // get 
        }

        /// <summary>
        /// Creates tags for constraints on the database
        /// </summary>
        private void parseConstraints() {
            //parsing constraints
            string query = "select k.name, k.type, k.type_desc, o.name from sys.key_constraints k inner join sys.objects o on k.parent_object_id = o.object_id order by k.type";
            using (SqlDataReader cons = dbUtility.executeQuery(query))
            {
                
                if (cons.HasRows)
                {
                    
                    while (cons.Read())
                    {
                        CommentNode n = new CommentNode();
                        string con = String.Format("{0}", cons[0]);
                        
                        n.addTag(new CommentTag("constraint", con));
                        n.addTag(new CommentTag("constraint type", String.Format("{0}",cons[1])));
                        n.addTag(new CommentTag("description", String.Format("{0}", cons[2])));
                        n.addTag(new CommentTag("parent table", String.Format("{0}", cons[3])));
                        nodes.Add(n);
                    }
                    
                }
                
            }
            query = "select fk.name, fk.type, fk.type_desc, o.name,	ref.name, fk.referenced_object_id, fk.parent_object_id, fk.object_id from sys.foreign_keys fk " +
                "inner join sys.objects o on fk.parent_object_id = o.object_id inner join sys.objects ref on fk.referenced_object_id = ref.object_id";
            using (SqlDataReader foreign = dbUtility.executeQuery(query)) {
                if (foreign.HasRows) {
                    while (foreign.Read()) {
                        CommentNode node = new CommentNode();
                        node.addTag(new CommentTag("constraint", foreign[0].ToString()));
                        node.addTag(new CommentTag("constraint type", foreign[1].ToString()));
                        node.addTag(new CommentTag("description", foreign[2].ToString()));

                        //adding parent table and constraint columns
                        node.addTag(new CommentTag("parent table", foreign[3].ToString()));
                        query = "select c.name from sys.foreign_key_columns k inner join sys.columns c on k.parent_column_id = c.column_id and k.parent_object_id = c.object_id "+
                            "where k.constraint_object_id=@conid and k.parent_object_id=@parent";
                        using (SqlDataReader cols = dbUtility.executeQuery(query, new SqlParameter("@conid", foreign[7]), 
                            new SqlParameter("@parent", foreign[6]))) {
                                if (cols.HasRows) {
                                    while (cols.Read()) { 
                                        node.addTag(new CommentTag("column", cols[0].ToString()));
                                    }
                                }
                        }

                        //adding referenced table and columns
                        node.addTag(new CommentTag("references", foreign[4].ToString()));
                        query = "select c.name from sys.foreign_key_columns k inner join sys.columns c on k.referenced_column_id = c.column_id and k.parent_object_id = c.object_id " +
                            "where k.constraint_object_id=@conid and k.referenced_object_id=@parent";
                        using (SqlDataReader cols = dbUtility.executeQuery(query, new SqlParameter("@conid", foreign[7]),
                            new SqlParameter("@parent", foreign[5])))
                        {
                            if (cols.HasRows)
                            {
                                while (cols.Read())
                                {
                                    node.addTag(new CommentTag("referenced column", cols[0].ToString()));
                                }
                            }
                        }
                        nodes.Add(node);
                    }
                }
            }
            //parse check constraints
            query = "select c.name, c.type, c.type_desc, o.name, col.name, c.definition from sys.check_constraints c inner join sys.objects o " +
                "on c.parent_object_id = o.object_id inner join sys.columns col on c.parent_column_id = col.column_id and o.object_id = col.object_id";
            using (SqlDataReader chk = dbUtility.executeQuery(query)) {
                if (chk.HasRows) {
                    while (chk.Read()) {
                        CommentNode node = new CommentNode();
                        node.addTag(new CommentTag("constraint", chk[0].ToString()));
                        node.addTag(new CommentTag("constraint type", chk[1].ToString()));
                        node.addTag(new CommentTag("description", chk[2].ToString()));
                        node.addTag(new CommentTag("parent table", chk[3].ToString()));
                        node.addTag(new CommentTag("column", chk[4].ToString()));
                        node.addTag(new CommentTag("definition", chk[5].ToString()));
                        nodes.Add(node);
                    }
                }
            }
            //parse default constraints
            query = "select d.name, d.type, d.type_desc, o.name, col.name, d.definition from sys.default_constraints d inner join sys.objects o on d.parent_object_id = o.object_id "
                + "inner join sys.columns col on d.parent_column_id = col.column_id and o.object_id = col.object_id";
            using (SqlDataReader def = dbUtility.executeQuery(query))
            {
                if (def.HasRows)
                {
                    while (def.Read())
                    {
                        CommentNode node = new CommentNode();
                        node.addTag(new CommentTag("constraint", def[0].ToString()));
                        node.addTag(new CommentTag("constraint type", def[1].ToString()));
                        node.addTag(new CommentTag("description", def[2].ToString()));
                        node.addTag(new CommentTag("parent table", def[3].ToString()));
                        node.addTag(new CommentTag("column", def[4].ToString()));
                        node.addTag(new CommentTag("definition", def[5].ToString()));
                        nodes.Add(node);
                    }
                }
            }
            //parsing foreign keys
            //parsing foreign keys' referenced tables
            //parsing foreign keys' columns
            //parse remaining constraints
        }

        /// <summary>
        /// Creates tags for the different functions on the database
        /// </summary>
        private void parseFunctions() {
            string func = "select o.object_id, o.name from sys.objects o where o.type = @type ";
            string help = "exec sp_helptext @func";
            //parse table functions
            using (SqlDataReader fn = dbUtility.executeQuery(func, new SqlParameter("@type", "IF"))){
                if (fn.HasRows) {

                    while (fn.Read()) {
                        CommentNode n = new CommentNode();
                        
                        n.addTag(new CommentTag("function", fn[1].ToString()));
                        List<CommentTag> parsed = new List<CommentTag>();
                        using (SqlDataReader ht = dbUtility.executeQuery(help, new SqlParameter("@func", fn[1].ToString()))) {
                            if (ht.HasRows) {
                                //
                                CommentTag tagInfo = null;
                                bool hascomment = false;
                                while (ht.Read()) {
                                    //parse tags inside function
                                    string line = ht[0].ToString();
                                    parseLine(line.Trim(), ref hascomment, ref tagInfo, ref parsed);
                                    n.filelines.Add(line);
                                }
                            }
                        }
                        if (parsed.Count > 0)
                        {
                            for (int i = 0; i < parsed.Count; i++)
                            {
                                Console.WriteLine(parsed[i].ToString());
                                CommentTag t = parsed[i];
                                if (!t.getTag().Equals("function"))
                                    n.addTag(t);
                            }
                            
                        }
                        List<CommentTag> missing = createMissingFunctionTags(fn[0]);
                        n.getTagList().AddRange(missing);
                        n.setFile(fn[1].ToString());
                        nodes.Add(n);
                    }
                }
            }
            //parse returning functions
            using (SqlDataReader fn = dbUtility.executeQuery(func, new SqlParameter("@type", "FN"))) {
                if (fn.HasRows)
                {
                    while (fn.Read())
                    {
                        CommentNode n = new CommentNode();
                        n.addTag(new CommentTag("function", fn[1].ToString()));
                        List<CommentTag> parsed = new List<CommentTag>();
                        using (SqlDataReader ht = dbUtility.executeQuery(help, new SqlParameter("@func", fn[1].ToString())))
                        {
                            
                            bool hascomment = false;
                            CommentTag tagInfo = null;
                            if (ht.HasRows)
                            {
                                while (ht.Read())
                                {
                                    string line = ht[0].ToString();
                                    parseLine(line, ref hascomment, ref tagInfo, ref parsed);
                                    n.filelines.Add(line);
                                }
                                for (int i = 0; i < n.filelines.Count; i++)
                                    Console.WriteLine(n.filelines[i]);
                            }
                        }
                        if (parsed.Count > 0)
                        {
                            for (int i = 0; i < parsed.Count; i++)
                            {
                                Console.WriteLine(parsed[i].ToString());
                                CommentTag t = parsed[i];
                                if (!t.getTag().Equals("function"))
                                    n.addTag(t);
                            }
                        }
                        else
                        {
                            //add manual tags for missing functions
                            List<CommentTag> t = createMissingFunctionTags(fn[0]);
                            n.getTagList().AddRange(t);
                        }
                        n.setFile(fn[1].ToString());
                        nodes.Add(n);
                    }
                }
            }
            
            
        }

        /// <summary>
        /// Creates tags for the different views on the database
        /// </summary>
        private void parseViews()
        {
            string func = "select o.object_id, o.name from sys.objects o where o.type = @type ";
            string help = "exec sp_helptext @func";
            //parse table functions
            using (SqlDataReader fn = dbUtility.executeQuery(func, new SqlParameter("@type", "V")))
            {
                if (fn.HasRows)
                {

                    while (fn.Read())
                    {
                        CommentNode n = new CommentNode();

                        n.addTag(new CommentTag("view", fn[1].ToString()));
                        List<CommentTag> parsed = new List<CommentTag>();
                        using (SqlDataReader ht = dbUtility.executeQuery(help, new SqlParameter("@func", fn[1].ToString())))
                        {
                            if (ht.HasRows)
                            {
                                //
                                CommentTag tagInfo = null;
                                bool hascomment = false;
                                while (ht.Read())
                                {
                                    //parse tags inside function
                                    string line = ht[0].ToString();
                                    parseLine(line.Trim(), ref hascomment, ref tagInfo, ref parsed);
                                    n.filelines.Add(line);
                                }
                            }
                        }
                        if (parsed.Count > 0)
                        {
                            for (int i = 0; i < parsed.Count; i++)
                            {
                                Console.WriteLine(parsed[i].ToString());
                                CommentTag t = parsed[i];
                                if (!t.getTag().Equals("function"))
                                    n.addTag(t);
                            }

                        }
                        List<CommentTag> missing = createMissingFunctionTags(fn[0]);
                        n.getTagList().AddRange(missing);
                        n.setFile(fn[1].ToString());
                        nodes.Add(n);
                    }
                }
            }
    }
        /// <summary>
        /// Creates tags for stored procedures
        /// </summary>
        private void parseStoredProcedures() {
            string proc = "select o.object_id, o.name from sys.objects o where o.type = @type ";
            string help = "exec sp_helptext @func";
            //parse table functions
            using (SqlDataReader fn = dbUtility.executeQuery(proc, new SqlParameter("@type", "P")))
            {
                if (fn.HasRows)
                {

                    while (fn.Read())
                    {
                        CommentNode n = new CommentNode();
                        n.addTag(new CommentTag("procedure", fn[1].ToString()));
                        List<CommentTag> parsed = new List<CommentTag>();
                        using (SqlDataReader ht = dbUtility.executeQuery(help, new SqlParameter("@func", fn[1].ToString())))
                        {
                            if (ht.HasRows)
                            {
                                //
                                CommentTag tagInfo = null;
                                bool hascomment = false;
                                while (ht.Read())
                                {
                                    //parse tags inside function
                                    string line = ht[0].ToString();
                                    parseLine(line.Trim(), ref hascomment, ref tagInfo, ref parsed);
                                    n.filelines.Add(line);
                                }
                            }
                        }
                        if (parsed.Count > 0)
                        {
                            for (int i = 0; i < parsed.Count; i++)
                            {
                                Console.WriteLine(parsed[i].ToString());
                                CommentTag t = parsed[i];
                                if (!t.getTag().Equals("procedure"))
                                    n.addTag(t);
                            }

                        }
                        
                        List<CommentTag> missing = createMissingFunctionTags(fn[0]);
                        n.getTagList().AddRange(missing);
                        
                        n.setFile(fn[1].ToString());
                        nodes.Add(n);
                    }
                }
            }
        }

        /// <summary>
        /// creates missing tags for function 
        /// </summary>
        /// <param name="id">Function identifier</param>
        /// <returns>A list of tags for function</returns>
        private List<CommentTag> createMissingFunctionTags(object id){
            //add manual tags for missing functions
            List<CommentTag> t = new List<CommentTag>();
            string pars = "select p.name, p.is_output, t.name, p.max_length from sys.parameters p inner join sys.objects o on p.object_id = o.object_id inner join sys.types t on p.system_type_id = t.system_type_id" +
                " where p.object_id = @oid";
            using (SqlDataReader p = dbUtility.executeQuery(pars, new SqlParameter("@oid", id)))
            {
                if (p.HasRows)
                    while (p.Read())
                    {
                        bool output = p.GetBoolean(1);
                        string paramName = p[0].ToString();
                        string parameter = null;
                        if (String.IsNullOrEmpty(paramName) || String.IsNullOrWhiteSpace(paramName))
                        {
                            parameter = String.Format("{0}({1})", p[2], p[3]);
                            t.Add(new CommentTag("returns", parameter));
                        }
                        else
                        {
                            parameter = String.Format("{0} {1}({2}) {3}", paramName, p[2], p[3], output ? "output" : null);
                            t.Add(new CommentTag("parameter", parameter));

                        }

                    }
            }
            //add type and type_desc tags
            pars = "select o.type, o.type_desc from sys.objects o where o.object_id = @oid";
            using (SqlDataReader ty = dbUtility.executeQuery(pars, new SqlParameter("@oid", id)))
            {
                if (ty.HasRows)
                    while (ty.Read())
                    {
                        t.Add(new CommentTag("object type", ty[0].ToString()));
                        t.Add(new CommentTag("description", ty[1].ToString()));
                    }
            }
            return t;
        }

        /// <summary>
        /// parses a commented text line according to algorithm
        /// </summary>
        /// <param name="line">Line to parse</param>
        /// <param name="hascomment">bool indicating a comment is being parsed</param>
        /// <param name="tagInfo">New tag for line </param>
        /// <param name="parsed">Tag list</param>
        private void parseLine(string line, ref bool hascomment, ref CommentTag tagInfo, ref List<CommentTag> parsed) {
            string beggining = @"^/[\*]{2}$";
            //original pattern on perl : /\*[\s]*@[\w]/ or /[\-]*[\s]*@[\w]/
            Regex tagline = new Regex(@"^\-{2}[\s]+|\*+?[\s]+");
            //original pattern on perl : /\*\//
            Regex tag = new Regex(@"(@[\w]*[\s]+)");
            string endcomment = @"\*/";
            if (Regex.IsMatch(line, beggining))
                hascomment = true;
            else if (tagline.IsMatch(line))
            {
                if (hascomment)
                {
                    if (tag.IsMatch(line))
                    {
                        string[] tagvalue = tag.Split(line, 2);
                        CommentTag temptag = new CommentTag(tagvalue[1].Substring(1).ToLowerInvariant().Trim(), tagvalue[2]);
                        tagInfo = temptag;
                        parsed.Add(tagInfo);
                    }
                    else
                    {
                        string[] addlines = tagline.Split(line, 2);
                        if(tagInfo != null)
                            tagInfo.setText(tagInfo.getText() + ' ' + addlines[1]);
                    }
                }
            }
            else if (Regex.IsMatch(line, endcomment))
                hascomment = false;
        }
    }
}
