import * as React from 'react';
import { RouteComponentProps } from 'react-router';

export class Home extends React.Component<RouteComponentProps<{}>, {}> {
    public render() {
        return <div>
            <h1>Introduction</h1>
            <p>What's the BSF website and application?</p>
        </div>;
    }
}
