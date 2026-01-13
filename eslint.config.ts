import type { Linter } from 'eslint'
import antfu from '@antfu/eslint-config'

const linters = antfu(
	{
		typescript: true,
		yaml: true,
		jsonc: true,
		markdown: false,
		formatters: {
			css: true,
			markdown: false,
			xml: true,
		},
		stylistic: {
			indent: 'tab',
			quotes: 'single',
		},
		ignores: [
			'**/dist',
			'**/node_modules',
			'**/*.js',
			'**/*.d.ts',
			'.vscode/.extensions',
			'.vscode/.user.data',
			'.vscode/.linux/.dotnet',
			'.pnpm-lock.yaml',
		],
		rules: {
			'unused-imports/no-unused-imports': 'warn',
		},
	},
)

const configs = await linters.toConfigs()

const antfuFormatterXml = configs.find(c => c.name === 'antfu/formatter/xml')

if (antfuFormatterXml) {
	// New MSBuild config creation based on the XML config
	const msbuildConfig = {
		...antfuFormatterXml,
		name: 'antfu/formatter/msbuild',
		// Only apply to MSBuild-related files.
		files: [
			'**/*.slnx',
			'**/*.props',
			'**/*.targets',
			'**/*.csproj',
			'**/*.csproj.user',
			'**/*.pubxml',
			'**/*.nuspec',
			'**/*.pkgproj',
		],
		rules: {
			...antfuFormatterXml.rules,
			'format/prettier': [
				'error',
				{
					...(
						Array.isArray(antfuFormatterXml.rules?.['format/prettier'])
						&& antfuFormatterXml.rules?.['format/prettier'].length > 1
							? antfuFormatterXml.rules?.['format/prettier'][1]
							: {}
					),
					// Extended Prettier config for MSBuild files
					printWidth: 500,
					tabWidth: 4,
					useTabs: false,
					xmlWhitespaceSensitivity: 'ignore',
					endOfLine: 'lf',
				},
			],
		},
	} as Linter.Config
	// Add MSBuild config
	linters.append(msbuildConfig)

	// New MSBuild config creation based on the XML config
	const xamlConfig = {
		...antfuFormatterXml,
		name: 'antfu/formatter/xaml',
		// Only apply to MSBuild-related files.
		files: [
			'**/*.xaml',
		],
		rules: {
			...antfuFormatterXml.rules,
			'format/prettier': [
				'error',
				{
					...(
						Array.isArray(antfuFormatterXml.rules?.['format/prettier'])
						&& antfuFormatterXml.rules?.['format/prettier'].length > 1
							? antfuFormatterXml.rules?.['format/prettier'][1]
							: {}
					),
					// Extended Prettier config for MSBuild files
					printWidth: 120,
					tabWidth: 4,
					useTabs: true,
					xmlWhitespaceSensitivity: 'ignore',
					endOfLine: 'crlf',
				},
			],
		},
	} as Linter.Config
	// Add MSBuild config
	linters.append(xamlConfig)
}

export default linters
