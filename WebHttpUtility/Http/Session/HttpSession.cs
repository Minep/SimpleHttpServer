using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Http.Session
{
    public class HttpSession 
    {
        public bool IsInvalidated { get; private set; } = false;
        public long CreationTime { get; }
        public long LastAccessTime { get; private set; }
        public int MaxLifeTime { get; set; }
        public Guid SessionID { get; private set; }

        private Dictionary<string, object> AssociateData;

        public HttpSession(Guid SessionID) {
            CreationTime = DateTime.Now.GetUnixTime();
            AssociateData = new Dictionary<string, object>();
        }

        public void SetAccessed() {
            LastAccessTime = DateTime.Now.GetUnixTime();
        }

        public bool IsAttributeExist(string name) {
            return AssociateData.ContainsKey(name);
        }

        internal bool _CheckExpired(long currenUnixTime) {
            return currenUnixTime - LastAccessTime >= MaxLifeTime;
        }

        public void BindAttribute(string name, object obj) {
            if (!AssociateData.ContainsKey(name)) {
                AssociateData.Add(name, obj);
            }
        }

        public object GetAttribute(string name) {
            if (AssociateData.ContainsKey(name)) {
                return AssociateData[name];
            }
            return null;
        }

        public void UnbindAttribute(string name) {
            if (AssociateData.ContainsKey(name)) {
                AssociateData.Remove(name);
            }
        }

        public void Invalidate() {
            IsInvalidated = true;
            AssociateData.Clear();
        }
    }
}
