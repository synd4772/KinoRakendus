﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KinoRakendus.core.enums;
using KinoRakendus.core.models;
using KinoRakendus.core.interfaces;
using KinoRakendus.core.models.database;
using System.Runtime.InteropServices;


namespace KinoRakendus.core.utils
{

    public static class DBHandler
    {
        public static string ConnectionString { get; private set; } = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=kinorakendus;Integrated Security=True";

        public static User CheckUser(string username, string password)
        {
            List<Kasutaja> users = GetTableData<Kasutaja>();
            foreach(Kasutaja user in users)
            {
                Console.WriteLine(user["nimi"]);
            }
            foreach(Kasutaja user in users)
            {
                
                User advancedUser = User.ConvertUser(user);
                if(advancedUser.Check(username, password))
                {
                    return advancedUser;
                }
            }
            return null;
        }
        public static string GetSingleResponse(string query, string field)
        {
            string response = string.Empty;

            using(SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            {
                            response = reader[field].ToString();
                            
                        }
                    }
                }
            }
            return response;
        }

        public static void AddUser(Kasutaja kasutaja)
        {
            string sql = $"INSERT INTO {kasutaja.tableName}(";
            int index = -1;
            List<string> fields = kasutaja.GetKeys();
            foreach(string field in fields)
            {
                index++;
                if (field != "id") sql += $"{field}{(fields.Count != index + 1 ? "," : "")}";
            }
            sql += ") VALUES(";
            index = -1;
            foreach(string field in fields)
            {
                index++;
                if (field != "id") sql += $"{ ConvertValue(kasutaja[field]) }{ ( fields.Count != index + 1 ? "," : "" ) }";
            }
            sql += ");";

            MakeQuery(sql);
        }
        public static List<string> CheckForForeign<T>(string fieldName) where T: Table, ITable, new()
        {
            List<string> returnList = new List<string>();
            T record = new T();
            string tableName = record.tableName;
            record = null;
            string query = $@"
            SELECT 
                fk.name AS ForeignKeyName,
                fk_cols.parent_object_id,
                OBJECT_NAME(fk.parent_object_id) AS TableName,
                col.name AS ColumnName,
                OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
                ref_col.name AS ReferencedColumn
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fk_cols
                ON fk.object_id = fk_cols.constraint_object_id
            INNER JOIN sys.columns col
                ON col.object_id = fk_cols.parent_object_id AND col.column_id = fk_cols.parent_column_id
            INNER JOIN sys.columns ref_col
                ON ref_col.object_id = fk_cols.referenced_object_id AND ref_col.column_id = fk_cols.referenced_column_id
            WHERE OBJECT_NAME(fk.parent_object_id) = @TableName
              AND col.name = @ColumnName;
            ";

            using(SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                Console.WriteLine($"LOLKKEEEEEEEEEEEK");
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@ColumnName", fieldName);
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            returnList.Add(reader["ReferencedTable"].ToString());
                            returnList.Add(reader["ReferencedColumn"].ToString());
                            Console.WriteLine($"{reader["ReferencedTable"]}: {reader["ReferencedColumn"]}");
                        }
                    }
                }

            }
            return returnList;

        }
        public static void UpdateUserData(Kasutaja kasutaja, string field, string value)
        {
            UpdateRecord(kasutaja, field, value, new List<WhereField>() { new WhereField("id", kasutaja["id"])});
        }
        public static void UpdateRecord<T>(T record, string field, string value, List<WhereField> whereFields) where T: Table, ITable
        {
            string sql = $"UPDATE {record.tableName} SET {field} = {ConvertValue(value)} WHERE ";
            
            int index = -1;
            foreach(WhereField whereField in whereFields)
            {
                index++;
                sql += $"{whereField.Field} = {whereField.Value}{(whereFields.Count == index + 1 ? ";" : "AND ")}";
            }
            Console.WriteLine(sql);
            MakeQuery(sql);
        }
        public static string ConvertValue(string value)
        {
            return ( value == null ? "NULL" : ( double.TryParse(value, out _) ? value : $"'{value}'") );
        }
        public static void MakeQuery(string query)
        {
            Console.WriteLine($"MakeQuery {query}");
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    Console.WriteLine(query);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static T GetRecord<T>(List<WhereField> whereFields) where T : Table, ITable, new()
        {
            T table = new T();
            string tableName = table.tableName;
            string sql = $"SELECT * FROM {tableName} WHERE ";
            
            int index = -1;
            foreach(WhereField whereField in whereFields)
            {
                index++;
                sql += $"{whereField.Field} = {ConvertValue(whereField.Value)} {(whereFields.Count == index + 1 ? ";" : "AND ")}";
            }
            Console.WriteLine($"SQL query for {tableName}, {sql}");
            using(SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(SqlCommand command = new SqlCommand(sql, connection))
                {
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreach(string tableField in table.GetKeys())
                            {
                                table[tableField] = reader[tableField].ToString();
                            }
                        }
                    }
                }
            }
            return table;
        }

        public static List<T> GetTableData<T>() where T : Table, ITable, new()
        {
            List<T> datas = new List<T>();
            string tableName = new T().tableName;
            string sql = $"SELECT * FROM {tableName}";
            
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
              
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    T data = new T();
                    List<string> values = new List<string>();
                    foreach(string field in data._data.Keys.ToList())
                    {
                        data[field] = dt.Rows[i][field].ToString();
                    }
                    datas.Add(data);
                    
                }
            }
            return datas;
        }
    }
}
