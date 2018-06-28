import * as React from 'react';
import * as SessionState from '../../store/Session';
import { NavLink, RouteComponentProps, withRouter } from 'react-router-dom';
import * as ReactDOM from 'react-dom';
import { bindActionCreators } from 'redux';
import { Dispatch, connect } from 'react-redux';
import { ApplicationState } from '../../store';
import { NavContext } from '../../App';
interface NavProps {
    onUpdate: () => void;
}

type AdminNavMenuProps = SessionState.SessionState

export class AdminNavMenu extends React.Component<AdminNavMenuProps, {}> {

    public render() {
        const { token } = this.props;

        if (token == undefined)
            return null

        if (Object.keys(token).length === 0)
            return null

        const { claims } = token;
        if (claims && claims.constructor === Array) {
            if (claims.some((claim) => { return claim == "Admin"; })) {
                return <NavContext.Consumer {...this.props}>
                    {({ onUpdate }: NavProps) => (
                        <li className="nav-item dropdown">
                            <a className="nav-link dropdown-toggle" href="http://example.com" id="dropdown04" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">Admin</a>
                            <div className="dropdown-menu" aria-labelledby="dropdown04">
                                <NavLink key="nav-admin-items" className="dropdown-item" to={'/admin/items'} onClick={onUpdate} activeClassName='active' href="">Items</NavLink>
                                <NavLink key="nav-admin-orders" className="dropdown-item" to={'/admin/orders'} onClick={onUpdate} activeClassName='active' href="">Orders</NavLink>
                                <NavLink key="nav-admin-users" className="dropdown-item" to={'/admin/users'} onClick={onUpdate} activeClassName='active' href="">Users</NavLink>
                            </div>
                        </li>
                    )}
                </NavContext.Consumer>
            }
        }

        return null
    }
}

export default AdminNavMenu;