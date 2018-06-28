import * as React from 'react';
import { SliderMenu } from '../Slider/SliderMenu';
import { NavContext } from '../../App';
import Alert from './AlertComponent';
import * as SessionState from '../../store/Session';
import * as AccountState from '../../store/Account';
import * as AlertState from '../../store/Alert';
import { ApplicationState } from '../../store';
import { RouteComponentProps } from 'react-router-dom';

interface NavProps {
    on: boolean;
    handleOverlayToggle: (e) => void;
}

type LayoutProps = ApplicationState
    & {
        accountActions: typeof AccountState.actionCreators,
        sessionActions: typeof SessionState.actionCreators,
        alertActions: typeof AlertState.actionCreators;
    }
    & RouteComponentProps<{}>;

export class Layout extends React.Component<LayoutProps, {}> {
    public render() {
        return <NavContext.Consumer>
            {({ on, handleOverlayToggle }: NavProps) => (
                <React.Fragment>
                    <main onClick={(e) => handleOverlayToggle(e)} className={`container ${on ? " overlay" : ""}`}>
                        <Alert {...this.props} />
                        <div id="slider" className={`row row-offcanvas row-offcanvas-right content ${on ? " active" : ""}`}>
                            <div className="col-12 col-md-12 col-lg-9">
                                {this.props.children}
                            </div>
                            <div id="sidebar" className="col-8 col-md-0 col-lg-3 sidebar-offcanvas">
                                <div className="list-group">
                                    <SliderMenu />
                                </div>
                            </div>
                        </div>
                    </main>
                </React.Fragment>)}
        </NavContext.Consumer>
    }
}

export default Layout;