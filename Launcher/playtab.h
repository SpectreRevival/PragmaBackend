#pragma once
#include <QWidget>
#include <QPropertyAnimation>
#include <vector>

QT_BEGIN_NAMESPACE
namespace Ui {
    class PlayTab;
}
QT_END_NAMESPACE

class PlayTab : public QWidget {
    Q_OBJECT

  public:
    explicit PlayTab(QWidget* parent = nullptr);
    ~PlayTab() override;

  private:
    Ui::PlayTab* ui;
    int currentCarouselIndex = 0;
    void StartCarouselAnimation();
    QPoint carouselInitialPosition;
    std::vector<QPixmap*> carouselImages;
};
