const path = require('path');
const TerserPlugin = require('terser-webpack-plugin')

module.exports = {
  entry: [
    path.resolve(__dirname, 'src/engine.ts')
  ],
  watch: false,
  output: {
    path: __dirname,
    filename: "../engine.js",
    chunkFilename: '../[name].js'
  },
  module: {
    rules: [
      {
        test: /\.js?$/,
        loader: 'babel-loader'
      },
      {
        test: /\.tsx?$/,
        loader: 'babel-loader'
      }
    ]
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js']
  },
  mode: 'production',
  optimization: {
    minimize: true,
    minimizer: [
      new TerserPlugin({
        parallel: true,
        exclude: 'meta-ui-grid.js'
      })
    ]
  },
  stats: {
    colors: true
  }
};