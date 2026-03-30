#if COMPACT_FRAMEWORK
namespace CrSceneFrameworkCF
#else
namespace CrSceneFramework
#endif
{
    public enum AuthorizationProcessState
    {
        Undecided,
        Granted,
        Redundant,
        Rejected,
        Cancelled
    }

    public abstract class AAuthorizationProcessBase
    {
        private AuthorizationProcessState _authorizationProcessState = AuthorizationProcessState.Undecided;

        protected AuthorizationProcessState CurrentAuthorizationProcessState
        {
            get { return _authorizationProcessState; }
            set { _authorizationProcessState = value; }
        }

        protected abstract bool AuthorizationByCodeEnabled
        {
            get;
        }

        public bool AcceptsCode
        {
            get
            {
                return CanAcceptCode();
            }
        }

        protected virtual bool CanAcceptCode()
        {
            return
                AuthorizationByCodeEnabled
                && _authorizationProcessState == AuthorizationProcessState.Undecided
                && CardData == null;
        }

        protected abstract bool AuthorizationByCardEnabled
        {
            get;
        }

        public bool AcceptsCard
        {
            get 
            { 
                return CanAcceptCard();
            }
        }

        protected virtual bool CanAcceptCard()
        {
            return
                AuthorizationByCardEnabled
                && _authorizationProcessState == AuthorizationProcessState.Undecided;
        }

        protected abstract bool CardRequiresPin
        {
            get;
        }

        public bool IsPinRequired
        {
            get
            {
                return GetIsPinRequired();
            }
        }

        protected virtual bool GetIsPinRequired()
        {
            return
                CardData != null
                && _authorizationProcessState == AuthorizationProcessState.Undecided
                && CardRequiresPin;
        }

        protected abstract bool AuthorizeByCard(out bool isRedundant);

        public void OnCardSwiped(
            string cardData,
            int cardSystemNumber)
        {
            CardData = cardData;
            CardSystemNumber = cardSystemNumber;

            bool isRedundant;

            var authorizationResult = AuthorizeByCard(out isRedundant);

            _authorizationProcessState =
                ResolveStateAfterCardAuthorization(
                    authorizationResult,
                    isRedundant);
        }

        protected abstract bool AuthorizeByCode(
            string codeData,
            out bool isRedundant);

        protected abstract bool AuthorizeByPin(string codeData);

        public void OnCodeSpecified(string codeData)
        {
            if (AcceptsCode)
            {
                bool isRedundant;
                var authorizationResult =
                    AuthorizeByCode(
                        codeData,
                        out isRedundant);

                _authorizationProcessState =
                    ResolveStateAfterCodeAuthorization(
                        authorizationResult,
                        isRedundant);

                return;
            }

            _authorizationProcessState =
                ResolveStateAfterPinAuthorization(
                    AuthorizeByPin(codeData));
        }

        protected virtual AuthorizationProcessState ResolveStateAfterCardAuthorization(
            bool authorizationResult,
            bool isRedundant)
        {
            if (!authorizationResult)
                return AuthorizationProcessState.Rejected;

            if (isRedundant)
                return AuthorizationProcessState.Redundant;

            return !CardRequiresPin
                ? AuthorizationProcessState.Granted
                : AuthorizationProcessState.Undecided;
        }

        protected virtual AuthorizationProcessState ResolveStateAfterCodeAuthorization(
            bool authorizationResult,
            bool isRedundant)
        {
            if (!authorizationResult)
                return AuthorizationProcessState.Rejected;

            return isRedundant
                ? AuthorizationProcessState.Redundant
                : AuthorizationProcessState.Granted;
        }

        protected virtual AuthorizationProcessState ResolveStateAfterPinAuthorization(
            bool authorizationResult)
        {
            return authorizationResult
                ? AuthorizationProcessState.Granted
                : AuthorizationProcessState.Rejected;
        }

        public virtual bool AdvanceOnUndecided
        {
            get { return true; }
        }

        public AuthorizationProcessState AuthorizationProcessState
        {
            get { return _authorizationProcessState; }
        }

        protected string CardData
        {
            get;
            private set;
        }

        protected int CardSystemNumber
        {
            get;
            private set;
        }

        public abstract int PinLength
        {
            get;
        }

        protected virtual void OnReset()
        {
        }

        public void Reset()
        {
            _authorizationProcessState = AuthorizationProcessState.Undecided;

            CardData = null;

            OnReset();
        }

        public void Cancel()
        {
            _authorizationProcessState = AuthorizationProcessState.Cancelled;
        }
    }
}
