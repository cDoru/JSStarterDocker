import * as React from "react";
import { Link, RouteComponentProps } from 'react-router-dom';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import { bindActionCreators, Dispatch } from 'redux';
import { AlertType, Field as ModelField, LoginViewModel } from '../../models';
import * as AccountState from '../../store/Account';
import * as SessionState from '../../store/Session';
import * as AlertState from '../../store/Alert';
import SigninForm from './SignInForm';
import { InjectedFormProps } from 'redux-form';

type UserMenuProps = AccountState.AccountState
    & {
        accountActions: typeof AccountState.actionCreators,
        alertActions: typeof AlertState.actionCreators,
        sessionActions: typeof SessionState.actionCreators,
    }
    & RouteComponentProps<{}>;

interface AdditionalProps {
    onCancel: () => void;
    fields: ModelField[];
    formButton?: string;
}

type FormProps = InjectedFormProps & AdditionalProps;

class SignIn extends React.Component<UserMenuProps, FormProps> {
    componentDidMount() {
        document.getElementsByTagName("input")[0].focus();
    }
    refs: {
        username: HTMLInputElement;
        password: HTMLInputElement;
    }

    render() {
        return <div className="container pt-4">
            <div className="row justify-content-center pt-4">
                <div className="col-12 col-sm-8 col-md-6 col-lg-5">
                    <h2 className="text-center display-4">Sign-In.</h2>
                    <SigninForm form='signinForm'
                        onSubmit={async (values: LoginViewModel) => {
                            if (values.email && values.password) {
                                this.props.accountActions.login(values,
                                    () => {
                                        this.props.history.push('/');
                                        this.props.alertActions.sendAlert('Signed in successfully!', AlertType.success, true);
                                        this.props.sessionActions.loadToken();
                                    },
                                    (error) => {
                                        this.props.alertActions.sendAlert(error.error_description, AlertType.danger, true);
                                    }
                                );
                            }
                        }} />
                    <div className="bottom text-center">
                        New here? <Link to="/register">Register</Link>
                    </div>
                </div>
            </div>
        </div>;
    }
}

export default connect(
    (state: ApplicationState) => state.account, // Selects which state properties are merged into the component's props
    (dispatch: Dispatch<AccountState.AccountState> | Dispatch<AlertState.AlertState> | Dispatch<SessionState.SessionState>) => { // Selects which action creators are merged into the component's props
        return {
            accountActions: bindActionCreators(AccountState.actionCreators, dispatch),
            alertActions: bindActionCreators(AlertState.actionCreators, dispatch),
            sessionActions: bindActionCreators(SessionState.actionCreators, dispatch),
        };
    },
)(SignIn) as typeof SignIn;


