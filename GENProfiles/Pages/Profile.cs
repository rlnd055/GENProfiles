using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace GENProfiles
{
    public class Profile
    {
        private static long maxNumber;
        public long Number { get; set; }
        public long PulseWidth { get; set; }
        public long Frequency { get; set; }
        public long ScanSpeed { get; set; }
        public long FocalDistance { get; set; }
        public long ShapeSize { get; set; }
        public long Power { get; set; }
        public long Active { get; set; }
        public string Name { get; set; }

        public bool Selected { get; set; } = false;

        public Profile() { }

        public Profile(long number, long pulseWidth, long frequency, long scanSpeed, long focalDistance, long shapeSize, long power, long active, string name, bool selected = false)
        {
            this.Number = number;
            this.PulseWidth = pulseWidth;
            this.Frequency = frequency;
            this.ScanSpeed = scanSpeed;
            this.FocalDistance = focalDistance;
            this.ShapeSize = shapeSize;
            this.Power = power;
            this.Active = active;
            this.Name = name;
            this.Selected = selected;
        }

        static private int ExecuteWrite(string query, Dictionary<string, object> args)
        {
            int numberOfRowsAffected;

            //setup the connection to the database
            using (var conn = new SQLiteConnection("Data Source=C:\\GENProfilesSrv\\Profiles.db"))
            {
                conn.Open();

                //open a new command
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    //set the arguments given in the query
                    foreach (var pair in args)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }

                    //execute the query and get the number of row affected
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }

                return numberOfRowsAffected;
            }
        }

        static private DataTable ExecuteRead(string query, Dictionary<string, object> args)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            using (var conn = new SQLiteConnection("Data Source=C:\\GENProfilesSrv\\Profiles.db"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (args != null)
                    {
                        foreach (KeyValuePair<string, object> entry in args)
                        {
                            cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                        }
                    }

                    var da = new SQLiteDataAdapter(cmd);

                    var dt = new DataTable();
                    da.Fill(dt);

                    da.Dispose();
                    return dt;
                }
            }
        }

        static public int AddProfile(Profile profile)
        {
            const string query = "INSERT INTO Profile(Number, PulseWidth, Frequency, ScanSpeed, FocalDistance , ShapeSize, Power, Active, Name) VALUES(@number, @pulseWidth, @frequency, @scanSpeed, @focalDistance, @shapeSize, @power, @active, @name)";

            //here we are setting the parameter values that will be actually 
            //replaced in the query in Execute method
            var args = new Dictionary<string, object>
            {
                {"@number", profile.Number},
                {"@pulseWidth", profile.PulseWidth},
                {"@frequency", profile.Frequency},
                {"@scanSpeed", profile.ScanSpeed},
                {"@focalDistance", profile.FocalDistance},
                {"@shapeSize", profile.ShapeSize},
                {"@power", profile.Power},
                {"@active", profile.Active},
                {"@name", profile.Name}
            };
            
            int added = ExecuteWrite(query, args);
            maxNumber += added;   // Always == 1 if successfull
            return added;
        }

        static public int EditProfile(Profile profile)
        {
            const string query = "UPDATE Profile SET PulseWidth = @pulseWidth, Frequency = @frequency, ScanSpeed = @scanSpeed, FocalDistance = @focalDistance, ShapeSize = @shapeSize, Power = @power, Active = @active, Name = @name WHERE Number = @number";

            //here we are setting the parameter values that will be actually 
            //replaced in the query in Execute method
            var args = new Dictionary<string, object>
            {
                {"@number", profile.Number},
                {"@pulseWidth", profile.PulseWidth},
                {"@frequency", profile.Frequency},
                {"@scanSpeed", profile.ScanSpeed},
                {"@focalDistance", profile.FocalDistance},
                {"@shapeSize", profile.ShapeSize},
                {"@power", profile.Power},
                {"@active", profile.Active},
                {"@name", profile.Name}
            };

            return ExecuteWrite(query, args);
        }

        static public int DeleteProfile(Profile profile)
        {
            const string query = "Delete from Profile WHERE Number = @number";

            //here we are setting the parameter values that will be actually 
            //replaced in the query in Execute method
            var args = new Dictionary<string, object>
            {
                {"@number", profile.Number}
            };
            maxNumber--;
            return ExecuteWrite(query, args);
        }

        static public int RenumProfiles(long deletedNum)
        {
            // Always call this method after a deletion to renumber profiles
            int numberOfRowsAffected = 0;

            var query = "UPDATE Profile SET Number = Number - 1 WHERE Number = @number";

            //setup the connection to the database
            using (var conn = new SQLiteConnection("Data Source=C:\\GENProfilesSrv\\Profiles.db"))
            {
                conn.Open();

                while (deletedNum < maxNumber + 1)
                {
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@number", deletedNum + 1);
                        numberOfRowsAffected += cmd.ExecuteNonQuery();
                    }
                    deletedNum++;
                }
            }
            return numberOfRowsAffected;
        }

        static public Profile GetProfileByNumber(long number)
        {
            var query = "SELECT * FROM Profile WHERE Number = @number";

            var args = new Dictionary<string, object>
            {
                {"@number", number}
            };

            DataTable dt = ExecuteRead(query, args);

            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            var profile = new Profile
            {
                Number = Convert.ToInt64(dt.Rows[0]["Number"]),
                PulseWidth = Convert.ToInt64(dt.Rows[0]["PulseWidth"]),
                Frequency = Convert.ToInt64(dt.Rows[0]["Frequency"]),
                ScanSpeed = Convert.ToInt64(dt.Rows[0]["ScanSpeed"]),
                FocalDistance = Convert.ToInt64(dt.Rows[0]["FocalDistance"]),
                ShapeSize = Convert.ToInt64(dt.Rows[0]["ShapeSize"]),
                Power = Convert.ToInt64(dt.Rows[0]["Power"]),
                Active = Convert.ToInt64(dt.Rows[0]["Active"]),
                Name = (string)dt.Rows[0]["Name"],
                Selected = false
            };

            return profile;
        }

        static public List<Profile> GetProfiles()
        {
            // Always call this method on startup to correctly set maxNumber
            maxNumber = 0;
            List<Profile> profiles = new List<Profile>();
            string query = "SELECT Number, PulseWidth, Frequency, ScanSpeed, FocalDistance, ShapeSize, Power, Active, Name FROM Profile ORDER BY Number";

            using (SQLiteConnection conn = new SQLiteConnection(@"Data Source=C:\\GENProfilesSrv\\Profiles.db;Pooling=true;FailIfMissing=false"))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = query;
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            IDataRecord idr = (IDataRecord)dr;
                            Profile profile = new Profile
                            {
                                Number = (long)idr[0],
                                PulseWidth = (long)idr[1],
                                Frequency = (long)idr[2],
                                ScanSpeed = (long)idr[3],
                                FocalDistance = (long)idr[4],
                                ShapeSize = (long)idr[5],
                                Power = (long)idr[6],
                                Active = (long)idr[7],
                                Name = (string)idr[8],
                                Selected = false
                            };
                            profiles.Add(profile);
                            if (profile.Number > maxNumber)
                                maxNumber = profile.Number;
                        }
                        return profiles;
                    }
                }
            }
        }

        static public long MaxNumber()
        {
            return maxNumber;
        }
    }
}
