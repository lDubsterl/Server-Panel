export function parseTranslations(filename, setTranslations) {
    fetch('/' + filename) // файл лежит в папке public
        .then((response) => response.text())
        .then((text) => {
            const lines = text.split('\n');
            const result = {};

            lines.forEach((line) => {
                const trimmedLine = line.trim();
                if (trimmedLine) {
                    const [key, value] = trimmedLine.split(':');
                    if (key && value)
                        result[key.trim()] = value.trim();
                }
            });
            setTranslations(result);
        });
}

export function parseTerrariaConfig(lines) {
    const parsed = [];
    let pendingOptions = null;
    let skipNextEntry = false;

    for (let line of lines) {
        const trimmed = line.trim();

        if (trimmed === '') {
            parsed.push({ type: 'raw', content: line });
            continue;
        }

        if (trimmed.startsWith('##')) {
            if (trimmed === '## off') {
                skipNextEntry = true;
                continue;
            }

            // parse options
            const comment = trimmed.slice(2).trim(); // remove "##"
            pendingOptions = comment.split('/').map(opt => {
                const optMatch = opt.match(/^([^\(]+)\(([^)]+)\)/);
                return optMatch
                    ? { value: optMatch[1].trim(), label: optMatch[2].trim() }
                    : { value: opt.trim(), label: opt.trim() };
            });

            continue;
        }

        if (trimmed.startsWith('#')) {
            parsed.push({ type: 'comment', content: line });
            continue;
        }

        const match = line.match(/^([^=]+)=([^\s#]*)(?:\s*#\s*(.+))?$/);
        if (match) {
            const key = match[1].trim();
            const value = match[2].trim();
            const comment = match[3]?.trim();

            const entry = {
                type: 'entry',
                key,
                default: value,
                comment,
                options: pendingOptions || null,
                hidden: skipNextEntry
            };

            parsed.push(entry);
            pendingOptions = null;
            skipNextEntry = false;
        } else {
            parsed.push({ type: 'raw', content: line });
        }
    }
    console.log(parsed);
    return parsed;
}

export function stringifyTerrariaConfig(parsed) {
    const lines = [];

    for (const entry of parsed) {
        if (entry.type === 'comment' || entry.type === 'raw') {
            lines.push(entry.content);
        } else if (entry.type === 'entry') {
            if (entry.hidden) {
                lines.push('## off');
            }

            if (entry.options) {
                const optionLine = '## ' + entry.options
                    .map(opt => `${opt.value}(${opt.label})`)
                    .join('/');
                lines.push(optionLine);
            }

            let line = `${entry.key}=${entry.default}`;
            if (!entry.options && entry.comment) {
                line += ` # ${entry.comment}`;
            }

            lines.push(line);
        }
    }
    console.log(lines);
    return lines;
}

export function parseWorldgenConfig(lines) {
    const result = {};
    const paramRegex = /^\s*([\w_]+)\s*=\s*(.*?),?\s*(--.*)?$/;

    for (let line of lines) {
        line = line.trim();

        // Пропуск строк с фигурными скобками или override
        if (
            line.includes('{') ||
            line.includes('}') ||
            line.includes('override')
        ) continue;

        // Пропуск комментариев
        if (line.startsWith('--')) continue;

        const match = line.match(paramRegex);
        if (match) {
            const key = match[1];
            const rawValue = match[2].trim();
            const comment = match[3];

            // Удаляем кавычки, если есть
            const value = rawValue.replace(/^"|"$/g, '');

            // Парсим массив возможных значений из комментария
            let options = [];
            if (comment) {
                const commentValues = comment.replace('--', '').trim();
                options = commentValues
                    .split(',')
                    .map(opt => opt.replace(/["']/g, '').trim())
                    .filter(Boolean);
            }

            result[key] = [value, options];
        }
    }

    return result;
}

export function stringifyWorldgenConfig(fields) {
    const lines = [];
    lines.push('return {');
    lines.push('\toverride_enabled = true,');

    if ('preset' in fields) {
        const [presetValue, presetOptions] = fields['preset'];
        const comment = presetOptions.length
            ? ` -- ${presetOptions.map(opt => `"${opt}"`).join(', ')}`
            : '';
        lines.push(`\tpreset = ${presetValue},\t\t\t${comment}`);
    }

    lines.push('\toverrides = {');

    const keys = Object.keys(fields).filter(k => k !== 'preset');
    for (const key of keys) {
        const [value, options] = fields[key];
        const comment = options.length
            ? ` -- ${options.map(opt => `"${opt}"`).join(', ')}`
            : '';
        lines.push(`\t\t${key} = "${value}",\t\t\t${comment}`);
    }

    lines.push('\t},');
    lines.push('}');
    return lines;
}