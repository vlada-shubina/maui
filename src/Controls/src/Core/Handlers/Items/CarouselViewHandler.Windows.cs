﻿using System;
using System.Collections;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.Maui.Controls.Platform;
using WApp = Microsoft.UI.Xaml.Application;
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		LoopableCollectionView _loopableCollectionView;
		ScrollViewer _scrollViewer;
		WScrollBarVisibility? _horizontalScrollBarVisibilityWithoutLoop;
		WScrollBarVisibility? _verticalScrollBarVisibilityWithoutLoop;

		protected override IItemsLayout Layout { get; }

		LinearItemsLayout CarouselItemsLayout => ItemsView?.ItemsLayout;
		WDataTemplate CarouselItemsViewTemplate => (WDataTemplate)WApp.Current.Resources["CarouselItemsViewDefaultTemplate"];

		protected override void ConnectHandler(ListViewBase nativeView)
		{
			ItemsView.Scrolled -= CarouselScrolled;
			ListViewBase.SizeChanged += InitialSetup;
			
			UpdateScrollBarVisibilityForLoop();

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(ListViewBase nativeView)
		{
			if (ItemsView != null)
				ItemsView.Scrolled -= CarouselScrolled;

			if (ListViewBase != null)
			{
				ListViewBase.SizeChanged -= InitialSetup;

				if (CollectionViewSource?.Source is ObservableItemTemplateCollection observableItemsSource)
					observableItemsSource.CollectionChanged -= OnCollectionItemsSourceChanged;
			}

			if (_scrollViewer != null)
			{
				_scrollViewer.ViewChanging -= OnScrollViewChanging;
				_scrollViewer.ViewChanged -= OnScrollViewChanged;
				_scrollViewer.SizeChanged -= InitialSetup;
			}

			base.DisconnectHandler(nativeView);
		}

		protected override void UpdateItemsSource()
		{
			var itemsSource = ItemsView.ItemsSource;

			if (itemsSource == null)
				return;

			var itemTemplate = ItemsView.ItemTemplate;

			if (itemTemplate == null)
				return;

			base.UpdateItemsSource();
		}

		protected override void UpdateItemTemplate()
		{
			if (Element == null || ListViewBase == null)
				return;

			ListViewBase.ItemTemplate = CarouselItemsViewTemplate;
		}

		protected override void OnScrollViewerFound(ScrollViewer scrollViewer)
		{
			base.OnScrollViewerFound(scrollViewer);

			_scrollViewer = scrollViewer;
			_scrollViewer.ViewChanging += OnScrollViewChanging;
			_scrollViewer.ViewChanged += OnScrollViewChanged;
			_scrollViewer.SizeChanged += InitialSetup;

			UpdateScrollBarVisibilityForLoop();
		}

		protected override ICollectionView GetCollectionView(CollectionViewSource collectionViewSource)
		{
			_loopableCollectionView = new LoopableCollectionView(base.GetCollectionView(collectionViewSource));

			if (Element is CarouselView cv && cv.Loop)
			{
				_loopableCollectionView.IsLoopingEnabled = true;
			}

			return _loopableCollectionView;
		}

		protected override ListViewBase SelectListViewBase()
		{
			return CreateCarouselListLayout(CarouselItemsLayout.Orientation);
		}

		protected override CollectionViewSource CreateCollectionViewSource()
		{
			var collectionViewSource = TemplatedItemSourceFactory.Create(Element.ItemsSource, Element.ItemTemplate, Element,
				GetItemHeight(), GetItemWidth(), GetItemSpacing(), MauiContext);

			if (collectionViewSource is ObservableItemTemplateCollection observableItemsSource)
				observableItemsSource.CollectionChanged += OnCollectionItemsSourceChanged;

			return new CollectionViewSource
			{
				Source = collectionViewSource,
				IsSourceGrouped = false
			};
		}

		protected override ItemsViewScrolledEventArgs ComputeVisibleIndexes(ItemsViewScrolledEventArgs args, ItemsLayoutOrientation orientation, bool advancing)
		{
			args = base.ComputeVisibleIndexes(args, orientation, advancing);

			if (ItemsView.Loop)
			{
				args.FirstVisibleItemIndex %= ItemCount;
				args.CenterItemIndex %= ItemCount;
				args.LastVisibleItemIndex %= ItemCount;
			}

			return args;
		}

		ListViewBase CreateCarouselListLayout(ItemsLayoutOrientation layoutOrientation)
		{
			UI.Xaml.Controls.ListView listView;

			if (layoutOrientation == ItemsLayoutOrientation.Horizontal)
			{
				listView = new FormsListView()
				{
					Style = (UI.Xaml.Style)WApp.Current.Resources["HorizontalCarouselListStyle"],
					ItemsPanel = (ItemsPanelTemplate)WApp.Current.Resources["HorizontalListItemsPanel"]
				};

				ScrollViewer.SetHorizontalScrollBarVisibility(listView, WScrollBarVisibility.Auto);
				ScrollViewer.SetVerticalScrollBarVisibility(listView, WScrollBarVisibility.Disabled);
			}
			else
			{
				listView = new FormsListView()
				{
					Style = (UI.Xaml.Style)WApp.Current.Resources["VerticalCarouselListStyle"]
				};

				ScrollViewer.SetHorizontalScrollBarVisibility(listView, WScrollBarVisibility.Disabled);
				ScrollViewer.SetVerticalScrollBarVisibility(listView, WScrollBarVisibility.Auto);
			}

			listView.Padding = WinUIHelpers.CreateThickness(ItemsView.PeekAreaInsets.Left, ItemsView.PeekAreaInsets.Top, ItemsView.PeekAreaInsets.Right, ItemsView.PeekAreaInsets.Bottom);

			return listView;
		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdateCurrentItem();
		}

		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdatePosition();
		}

		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdateIsBounceEnabled();
		}

		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdateIsSwipeEnabled();
		}

		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdatePeekAreaInsets();
		}

		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView) 
		{
			handler.UpdateLoop();
		}

		void UpdateIsBounceEnabled()
		{
			if (_scrollViewer != null)
				_scrollViewer.IsScrollInertiaEnabled = ItemsView.IsBounceEnabled;
		}

		void UpdateIsSwipeEnabled()
		{
			ListViewBase.IsSwipeEnabled = ItemsView.IsSwipeEnabled;

			switch (CarouselItemsLayout.Orientation)
			{
				case ItemsLayoutOrientation.Horizontal:
					ScrollViewer.SetHorizontalScrollMode(ListViewBase, ItemsView.IsSwipeEnabled ? WScrollMode.Auto : WScrollMode.Disabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(ListViewBase, ItemsView.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Disabled);
					break;
				case ItemsLayoutOrientation.Vertical:
					ScrollViewer.SetVerticalScrollMode(ListViewBase, ItemsView.IsSwipeEnabled ? WScrollMode.Auto : WScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollBarVisibility(ListViewBase, ItemsView.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Disabled);
					break;
			}
		}

		void UpdatePeekAreaInsets()
		{
			ListViewBase.Padding = WinUIHelpers.CreateThickness(ItemsView.PeekAreaInsets.Left, ItemsView.PeekAreaInsets.Top, ItemsView.PeekAreaInsets.Right, ItemsView.PeekAreaInsets.Bottom);
			UpdateItemsSource();
		}

		void UpdateLoop()
		{
			UpdateScrollBarVisibilityForLoop();
			UpdateItemsSource();
		}

		double GetItemWidth()
		{
			var itemWidth = ListViewBase.ActualWidth;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				itemWidth = ListViewBase.ActualWidth - ItemsView.PeekAreaInsets.Left - ItemsView.PeekAreaInsets.Right;
			}

			return Math.Max(itemWidth, 0);
		}

		double GetItemHeight()
		{
			var itemHeight = ListViewBase.ActualHeight;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				itemHeight = ListViewBase.ActualHeight - ItemsView.PeekAreaInsets.Top - ItemsView.PeekAreaInsets.Bottom;
			}

			return Math.Max(itemHeight, 0);
		}

		Thickness? GetItemSpacing()
		{
			var itemSpacing = CarouselItemsLayout.ItemSpacing;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				return new Thickness(itemSpacing, 0, 0, 0);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				return new Thickness(0, itemSpacing, 0, 0);

			return new Thickness(0);
		}

		bool IsValidPosition(int position)
		{
			if (ItemCount == 0)
				return false;

			if (position < 0 || position >= ItemCount)
				return false;

			return true;
		}

		void SetCarouselViewPosition(int position)
		{
			if (ItemCount == 0)
			{
				return;
			}

			if (!IsValidPosition(position))
				return;

			var currentPosition = ItemsView.Position;

			if (currentPosition != position)
				ItemsView.Position = position;
		}

		void SetCarouselViewCurrentItem(int carouselPosition)
		{
			if (!IsValidPosition(carouselPosition))
				return;

			if (!(GetItem(carouselPosition) is ItemTemplateContext itemTemplateContext))
				throw new InvalidOperationException("Visible item not found");

			var item = itemTemplateContext.Item;
			ItemsView.CurrentItem = item;
		}

		int GetItemPositionInCarousel(object item)
		{
			for (int n = 0; n < ItemCount; n++)
			{
				if (GetItem(n) is ItemTemplateContext pair)
				{
					if (pair.Item == item)
					{
						return n;
					}
				}
			}

			return -1;
		}

		void UpdateCarouselViewInitialPosition()
		{
			if (ListViewBase.Items.Count > 0)
			{
				if (Element.Loop)
				{
					var item = ListViewBase.Items[0];
					_loopableCollectionView.CenterMode = true;
					ListViewBase.ScrollIntoView(item);
					_loopableCollectionView.CenterMode = false;
				}

				if (ItemsView.CurrentItem != null)
					UpdateCurrentItem();
				else
					UpdatePosition();
			}
		}

		void UpdateCurrentItem()
		{
			if (CollectionViewSource == null)
				return;

			var currentItemPosition = GetItemPositionInCarousel(ItemsView.CurrentItem);

			if (currentItemPosition != -1)
				ItemsView.ScrollTo(currentItemPosition, position: ScrollToPosition.Center, animate: ItemsView.AnimateCurrentItemChanges);
		}

		void UpdatePosition()
		{
			if (CollectionViewSource == null)
				return;

			var carouselPosition = ItemsView.Position;

			if (carouselPosition >= ItemCount || carouselPosition < 0)
				throw new IndexOutOfRangeException($"Can't set CarouselView to position {carouselPosition}. ItemsSource has {ItemCount} items.");

			SetCarouselViewCurrentItem(carouselPosition);
		}

		void UpdateScrollBarVisibilityForLoop()
		{
			if (_scrollViewer == null)
			{
				return;
			}

			if (Element.Loop)
			{
				// Track the current scrollbar settings
				_horizontalScrollBarVisibilityWithoutLoop = _scrollViewer.HorizontalScrollBarVisibility;
				_verticalScrollBarVisibilityWithoutLoop = _scrollViewer.VerticalScrollBarVisibility;

				// Disable the scroll bars, they don't make sense when looping
				_scrollViewer.HorizontalScrollBarVisibility = WScrollBarVisibility.Hidden;
				_scrollViewer.VerticalScrollBarVisibility = WScrollBarVisibility.Hidden;
			}
			else
			{
				// Restore the previous visibility (if any was recorded)
				if (_horizontalScrollBarVisibilityWithoutLoop.HasValue)
				{
					_scrollViewer.HorizontalScrollBarVisibility = _horizontalScrollBarVisibilityWithoutLoop.Value;
				}

				if (_verticalScrollBarVisibilityWithoutLoop.HasValue)
				{
					_scrollViewer.VerticalScrollBarVisibility = _verticalScrollBarVisibilityWithoutLoop.Value;
				}
			}
		}

		void CarouselScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			var position = e.CenterItemIndex;

			if (position == -1)
			{
				return;
			}

			if (position == Element.Position)
			{
				return;
			}

			SetCarouselViewPosition(position);
		}

		void OnScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
		{
			ItemsView.SetIsDragging(true);
			ItemsView.IsScrolling = true;
		}

		void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			ItemsView.SetIsDragging(e.IsIntermediate);
			ItemsView.IsScrolling = e.IsIntermediate;
		}

		void OnCollectionItemsSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var carouselPosition = ItemsView.Position;
			var currentItemPosition = GetItemPositionInCarousel(ItemsView.CurrentItem);
			var count = (sender as IList).Count;

			bool removingCurrentElement = currentItemPosition == -1;
			bool removingLastElement = e.OldStartingIndex == count;
			bool removingFirstElement = e.OldStartingIndex == 0;
			bool removingCurrentElementButNotFirst = removingCurrentElement && removingLastElement && ItemsView.Position > 0;

			if (removingCurrentElementButNotFirst)
			{
				carouselPosition = ItemsView.Position - 1;

			}
			else if (removingFirstElement && !removingCurrentElement)
			{
				carouselPosition = currentItemPosition;
			}

			// If we are adding a new item make sure to maintain the CurrentItemPosition
			else if (e.Action == NotifyCollectionChangedAction.Add
				&& currentItemPosition != -1)
			{
				carouselPosition = currentItemPosition;
			}

			SetCarouselViewCurrentItem(carouselPosition);
			SetCarouselViewPosition(carouselPosition);
		}

		void InitialSetup(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
			{
				ListViewBase.SizeChanged -= InitialSetup;

				if (_scrollViewer != null)
					_scrollViewer.SizeChanged -= InitialSetup;

				UpdateItemsSource();
				UpdateCarouselViewInitialPosition();
			}
		}
	}
}