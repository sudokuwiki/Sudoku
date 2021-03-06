﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sudoku.Drawing;
using Sudoku.Drawing.Extensions;
using Sudoku.Extensions;
using Sudoku.Windows.Drawing.Layers;
using static System.Reflection.BindingFlags;

namespace Sudoku.Windows
{
	partial class MainWindow
	{
		private void ImageGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is Image image))
			{
				e.Handled = true;
				return;
			}

			int getCell() => _pointConverter.GetCellOffset(e.GetPosition(image).ToDPointF());
			int getCandidate() => _pointConverter.GetCandidateOffset(e.GetPosition(image).ToDPointF());
			switch (Keyboard.Modifiers)
			{
				case ModifierKeys.None:
				{
					if (_currentColor == int.MinValue)
					{
						_focusedCells.Clear();
						_focusedCells.Add(getCell());
					}
					else
					{
						switch (_customDrawingMode)
						{
							case 0: // Cell.
							{
								int cell = getCell();
								if (_view.ContainsCell(cell))
								{
									_view.RemoveCell(cell);
								}
								else
								{
									_view.AddCell(_currentColor, cell);
								}

								break;
							}
							case 1: // Candidate.
							{
								int candidate = getCandidate();
								if (_view.ContainsCandidate(candidate))
								{
									_view.RemoveCandidate(candidate);
								}
								else
								{
									_view.AddCandidate(_currentColor, candidate);
								}

								break;
							}
						}

						_layerCollection.Remove(typeof(FocusLayer).Name);
						_layerCollection.Add(
							new CustomViewLayer(
								_pointConverter, _view, null, Settings.PaletteColors,
								Settings.EliminationColor, Settings.CannibalismColor, Settings.ChainColor));

						UpdateImageGrid();
					}

					break;
				}
				//case ModifierKeys.Alt:
				//{
				//	break;
				//}
				case ModifierKeys.Control:
				{
					// Multi-select.
					_focusedCells.Add(getCell());

					break;
				}
				case ModifierKeys.Shift:
				{
					// Select a region of cells.
					int cell = _focusedCells.IsEmpty ? 0 : _focusedCells.SetAt(0);
					int currentClickedCell = getCell();
					int r1 = cell / 9, c1 = cell % 9;
					int r2 = currentClickedCell / 9, c2 = currentClickedCell % 9;
					int minRow = Math.Min(r1, r2), minColumn = Math.Min(c1, c2);
					int maxRow = Math.Max(r1, r2), maxColumn = Math.Max(c1, c2);
					for (int r = minRow; r <= maxRow; r++)
					{
						for (int c = minColumn; c <= maxColumn; c++)
						{
							_focusedCells.Add(r * 9 + c);
						}
					}

					break;
				}
				//case ModifierKeys.Windows:
				//{
				//	break;
				//}
			}

			_layerCollection.Add(new FocusLayer(_pointConverter, _focusedCells, Settings.FocusedCellColor));

			UpdateImageGrid();
		}

		private void ImageUndoIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
			MenuItemEditUndo_Click(sender, e);

		private void ImageRedoIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
			MenuItemEditRedo_Click(sender, e);

		private void ImageGeneratingIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			((Action<object, RoutedEventArgs>)(_comboBoxMode.SelectedIndex switch
			{
				0 => (sender, e) => MenuItemGenerateWithSymmetry_Click(sender, e),
				1 => (sender, e) => MenuItemGenerateHardPattern_Click(sender, e),
				_ => throw Throwing.ImpossibleCase
			}))(sender, e);
		}

		private void ImageSolve_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
			MenuItemAnalyzeAnalyze_Click(sender, e);

		private void ImageGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			_currentRightClickPos = e.GetPosition(_imageGrid);

			// Disable all menu items.
			for (int i = 0; i < 9; i++)
			{
				((MenuItem)GetType()
					.GetField($"_menuItemImageGridSet{i + 1}", NonPublic | Instance)!.GetValue(this)!).IsEnabled = false;
				((MenuItem)GetType()
					.GetField($"_menuItemImageGridDelete{i + 1}", NonPublic | Instance)!.GetValue(this)!
				).IsEnabled = false;
			}

			// Then enable some of them.
			foreach (int i in
				_puzzle.GetCandidatesReversal(
					_pointConverter.GetCellOffset(_currentRightClickPos.ToDPointF())).GetAllSets())
			{
				((MenuItem)GetType()
					.GetField($"_menuItemImageGridSet{i + 1}", NonPublic | Instance)!.GetValue(this)!).IsEnabled = true;
				((MenuItem)GetType()
					.GetField($"_menuItemImageGridDelete{i + 1}", NonPublic | Instance)!.GetValue(this)!
				).IsEnabled = true;
			}
		}
	}
}
