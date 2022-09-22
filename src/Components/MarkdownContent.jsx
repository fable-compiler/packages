import React from 'react'
import ReactMarkdown from 'react-markdown'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import oneLight from './one-light'
import remarkGfm from 'remark-gfm'

const MarkdownContent = ({ content }) => {
    return <ReactMarkdown
        children={content}
        remarkPlugins={[remarkGfm]}
        components={{
            code({ node, inline, className, children, ...props }) {
                const match = /language-(\w+)/.exec(className || '')
                if (!inline && match) {
                    let language = match[1];
                    // Support language aliases
                    switch (language) {
                        case 'fs':
                            language = 'fsharp';
                            break;
                        default:
                            break;
                    }
                    return <SyntaxHighlighter
                        children={String(children).replace(/\n$/, '')}
                        style={oneLight}
                        language={language}
                        PreTag="div"
                        {...props}
                    />
                } else {
                    return <code className={className} {...props}>
                        {children}
                    </code>
                }
            }
        }}
    />
}
export default MarkdownContent;
