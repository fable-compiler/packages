import React from 'react';
import PropTypes from 'prop-types';
import Clipboard from "clipboard"
import { Copy } from "lucide-react";

class CopyInstruction extends React.Component {
    constructor(props) {
        super(props);
        this.state = { showSuccess: false };
    }

    componentDidMount() {
        this.clipboard = new Clipboard(this.element);

        const self = this;
        this.clipboard.on('success', function (e) {
            self.setState({ showSuccess: true });
        });
    }

    onMouseLeave = () => {
        if (this.state.showSuccess) {
            this.setState({ showSuccess: false });
        }
    }

    render() {
        return (
            <div className={`copy-instruction button is-primary`}
                data-tooltip={this.state.showSuccess ? 'Copied' : undefined}
                ref={(element) => { this.element = element; }}
                data-clipboard-text={this.props.value}
                onMouseLeave={this.onMouseLeave}>
                <Copy size={24}></Copy>
            </div>
        );
    }
}

CopyInstruction.propTypes = {
    value: PropTypes.string,
};

CopyInstruction.defaultProps = {
    value: "",
};

export default CopyInstruction;
