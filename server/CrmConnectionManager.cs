using MattermostCrmService.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattermostCrmService
{
    public class CrmConnectionManager
    {
        private static CrmConnectionManager m_instance = null;
        private Dictionary<string, CrmWrapper> m_cache = new Dictionary<string, CrmWrapper>(StringComparer.InvariantCultureIgnoreCase); 
        private object m_lock = new object();
        private CrmWrapper m_mattermostInstance = null;
        private string m_url = null;

        public static CrmConnectionManager Init(string username, string password, string url)
        {
            m_instance = new CrmConnectionManager(username, password, url);
            return m_instance;
        }

        public CrmConnectionManager(string username, string password, string url)
        {
            m_url = url;
            m_mattermostInstance = new CrmWrapper(username, password, url);
            m_cache.Add(username, m_mattermostInstance);
        }

        public static CrmConnectionManager Instance { get { return m_instance; } }

        public CrmWrapper MattermostInstance
        {
            get
            {
                return m_mattermostInstance;
            }
        }

        public CrmWrapper Get(AuthenticatedRequestBase request)
        {
            return Get(request.AuthenticationToken);
        }

        public void ReconnectAll()
        {
            lock (m_lock)
            {
                foreach (var user in m_cache.Keys.ToArray())
                {
                    try
                    {
                        m_cache[user].Reconnect();
                    }
                    catch (Exception e)
                    {
                        m_cache.Remove(user);
                    }
                }
            }
        }

        public CrmWrapper Get(string authenticationToken)
        {
            lock (m_lock)
            {
                var authInfo = LoginHelper.Instance.ParseToken(authenticationToken);
                if (authInfo == null) 
                    throw new ApplicationException("No Valid Auth Token");

                if (m_cache.ContainsKey(authInfo.Username))
                    return m_cache[authInfo.Username];

                var crm = new CrmWrapper(authInfo.Username, authInfo.Password, m_url);
                m_cache.Add(authInfo.Username, crm);
                return crm;
            }
        }

    }
}
